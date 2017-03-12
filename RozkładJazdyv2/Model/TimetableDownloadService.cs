using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RozkładJazdyv2.Model
{
    public class TimetableDownloadService : EventHelper
    {
        private const string _TIMETABLE_URL = "http://rozklady.kzkgop.pl/index.php?co=rozklady";
        private const string _TIMETABLE_BASE_URL = "http://rozklady.kzkgop.pl/";

        private TimetableDownloadService() { }

        public async static Task<bool> DownloadNewTimetableAsync()
        {
            List<Line> lines = await GetInfoAboutLinesAsync();
            bool areLinesDownloadedCorrectly = !(lines == null || lines.Count() == 0);
            return areLinesDownloadedCorrectly;
        }

        private async static Task<List<Line>> GetInfoAboutLinesAsync()
        {
            using (var htmlDocument = await GetAngleSharpDocumentOfSiteAsync(_TIMETABLE_URL))
            {
                if (htmlDocument == null)
                    return null;
                List<Line> lines = GetLinesInfoFromAngleSharpDocument(htmlDocument);
                if (lines == null || lines.Count() == 0)
                    return null;
                InvokeOnLinesInfoDownloaded();
                lines = await GetAllLinesDetailFromLinesInfo(lines);
                if (lines == null || lines.Count() == 0)
                    return null;
                return lines;
            }
        }

        private async static Task<List<Line>> GetAllLinesDetailFromLinesInfo(List<Line> lines)
        {
            int countOfLines = lines.Count();
            for (int i = 0; i < countOfLines; i++)
            {
                Line line = lines[i];
                line = await GetLineDetailInfo(line);
                if (line == null)
                    return null;
                lines[i] = line;
                InvokeOnLineDownloaded(line, countOfLines);
            }
            return lines;
        }

        private async static Task<Line> GetLineDetailInfo(Line line)
        {
            string url = string.Format("{0}{1}", _TIMETABLE_BASE_URL, line.Url);
            IElement htmlDocument = (await GetAngleSharpDocumentOfSiteAsync(url)).Children[0];
            await AddSchedulesToLineAsync(line, htmlDocument, url);
            return line;
        }

        private async static Task<Line> GetSchedulesDetailInfo(Line line)
        {
            int countOfSchedules = line.Schedules.Count();
            for(int i = 0; i < countOfSchedules; i++)
            {
                Schedule schedule = line.Schedules[i];
                var htmlDocument = await GetAngleSharpDocumentOfSiteAsync(schedule.Url);
                await GetTracksOfScheduleAngleSharpDocument(htmlDocument);
            }

            return line;
        }

        private async static Task<List<Track>> GetTracksOfScheduleAngleSharpDocument(IHtmlDocument htmlDocument)
        {
            throw new NotImplementedException();
        }

        private static async Task<Line> AddSchedulesToLineAsync(Line line, IElement htmlDocument, string url)
        {
            List<IElement> listOfHtmlSchedules = htmlDocument.QuerySelectorAll("td")
                      .Where(p => p.ClassName != null && p.ClassName.Contains("kier")).ToList();

            if (CheckIfLineHasSchedulesOrIsFreezed(listOfHtmlSchedules))
            {
                listOfHtmlSchedules = htmlDocument.QuerySelectorAll("div").Where(p => p.GetAttribute("id") != null &&
                                        p.GetAttribute("id").Contains("div_tabelki_tra")).ToList();
                if (CheckIfLineHasSchedulesOrIsFreezed(listOfHtmlSchedules))
                {
                    line.Schedules = new List<Schedule>().Add<Schedule>(
                                        new Schedule() { Name = "linia zawieszona" });
                    return line;
                }
                GetSchedulesDetail(ref line, listOfHtmlSchedules);
            }
            else
                line.Schedules = new List<Schedule>().Add<Schedule>(
                                new Schedule()
                                {
                                    Name = "obecnie obowiązujący",
                                    IdOfLine = line.Id,
                                    Url = url,
                                    IsActualSchedule = true
                                });
            line = await GetSchedulesDetailInfo(line);
            return line;
        }

        private static void GetSchedulesDetail(ref Line line, IEnumerable<IElement> listOfHtmlSchedules)
        {
            List<Schedule> listOfSchedules = new List<Schedule>();
            var schedules = listOfHtmlSchedules.ElementAt(0).Children[1].QuerySelectorAll("li a");
            int id = 0;
            foreach(var schedule in schedules)
            {
                listOfSchedules.Add(new Schedule()
                {
                    Id = id++,
                    IdOfLine = line.Id,
                    IsActualSchedule = schedule.TextContent.Contains("obecnie"),
                    Name = schedule.TextContent,
                    Url = string.Format("{0}{1}", _TIMETABLE_BASE_URL, schedule.GetAttribute("href"))
                });
            }
            line.Schedules = listOfSchedules;
        }

        private static List<Line> GetLinesInfoFromAngleSharpDocument(IHtmlDocument angleSharpDocument)
        {
            var listOfHtmlLines = angleSharpDocument.QuerySelectorAll("div")
                .Where(p => p.ClassName == "zbior_linii" && p.FirstElementChild != null)
                    .SelectMany(p => p.Children);
            if (listOfHtmlLines == null || listOfHtmlLines.Count() == 0)
                return null;
            List<Line> listOfLines = GetLinesInfoDetailFromAngleSharpCollection(listOfHtmlLines);
            if (listOfLines.Count() == 0)
                return null;
            return listOfLines;
        }

        private static List<Line> GetLinesInfoDetailFromAngleSharpCollection(IEnumerable<IElement> collection)
        {
            List<Line> listOfLines = new List<Line>();
            int type = 1 << 0;
            int id = 0;
            foreach (var element in collection)
            {
                type = GetLineSumBitFromType(element.FirstElementChild.ClassName);
                if ((type & (1 << 12)) == (1 << 12)) //if is 'koleje'
                    continue;

                Line line = new Line()
                {
                    Id = id++,
                    Url = @element.GetAttribute("href"),
                    Name = element.FirstElementChild.TextContent,
                    Type = type
                };
                listOfLines.Add(line);
            }
            return listOfLines;
        }

        private async static Task<IHtmlDocument> GetAngleSharpDocumentOfSiteAsync(string url)
        {
            string htmlOfSite = await HtmlService.GetHtmlFromSite(url);
            if (htmlOfSite == "")
                return null;
            return await new HtmlParser().ParseAsync(htmlOfSite);
        }

        private static int GetLineSumBitFromType(string type)
        {
            int bit = 1 << 0;
            if (type.Contains("zwykly"))
                bit |= 1 << 1;
            if (type.Contains("przysp"))
                bit |= 1 << 3;
            if (type.Contains("tram"))
                bit |= 1 << 4;
            if (type.Contains("mini"))
                bit |= 1 << 5;
            if (type.Contains("lot"))
                bit |= 1 << 6;
            if (type.Contains("swiezy"))
                bit |= 1 << 7;
            if (type.Contains("zast"))
                bit |= 1 << 8;
            if (type.Contains("wiekszy"))
                bit |= 1 << 9;
            if (type.Contains("nocna"))
                bit |= 1 << 10;
            if (type.Contains("bezp"))
                bit |= 1 << 11;
            if (type.Contains("kolej"))
                bit |= 1 << 12;
            return bit;
        }

        private static bool CheckIfLineHasSchedulesOrIsFreezed(List<IElement> listOfHtmlSchedules)
            => listOfHtmlSchedules == null || listOfHtmlSchedules.Count() == 0;
    }
}
