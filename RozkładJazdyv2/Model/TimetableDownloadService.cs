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
            using (var document = await GetAngleSharpDocumentOfSiteAsync(_TIMETABLE_URL))
            {
                if (document == null)
                    return null;
                List<Line> lines = GetLinesInfoFromAngleSharpDocument(document);
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
            }
            return lines;
        }

        private async static Task<Line> GetLineDetailInfo(Line line)
        {
            string url = string.Format("{0}{1}", _TIMETABLE_BASE_URL, line.Url);
            var document = (await GetAngleSharpDocumentOfSiteAsync(url)).Children[0];
            var listOfHtmlSchedules = document.QuerySelectorAll("td")
                        .Where(p => p.ClassName != null && p.ClassName.Contains("kier")).ToList();
            if (CheckIfLineHasSchedules(listOfHtmlSchedules))
            {
                listOfHtmlSchedules = document.QuerySelectorAll("div").Where(p => p.GetAttribute("id") != null && 
                                        p.GetAttribute("id").Contains("div_tabelki_tra")).ToList();
                if (CheckIfScheduleIsFreezed(listOfHtmlSchedules))
                {
                    line.Schedules = new List<Schedule>().Add<Schedule>(
                                        new Schedule() { Name = "linia zawieszona" });
                    return line;
                }
                AddSchedulesToLine(ref line, listOfHtmlSchedules);
            }
            else
                line.Schedules = new List<Schedule>().Add<Schedule>(
                                new Schedule() { Name = "obecnie obowiązujący", IdOfLine = line.Id,
                                    Url = url, IsActualSchedule = true });
            line = await GetSchedulesDetailInfo(line);
            
            return line;
        }

        private async static Task<Line> GetSchedulesDetailInfo(Line line)
        {
            foreach(var schedule in line.Schedules)
            {
                var document = GetAngleSharpDocumentOfSiteAsync(schedule.Url);
                //todo add downloading track
            }

            return line;
        }

        private static void AddSchedulesToLine(ref Line line, IEnumerable<IElement> listOfHtmlSchedules)
        {
            List<Schedule> listOfSchedules = new List<Schedule>();
            var schedules = listOfHtmlSchedules.ElementAt(0).Children[1].QuerySelectorAll("li a");
            foreach(var schedule in schedules)
            {
                listOfSchedules.Add(new Schedule(true)
                {
                    IdOfLine = line.Id,
                    IsActualSchedule = schedule.TextContent.Contains("obecnie"),
                    Name = schedule.TextContent,
                    Url = string.Format("{0}{1}", _TIMETABLE_BASE_URL, schedule.GetAttribute("href"))
                });
            }
            line.Schedules = listOfSchedules;
        }

        private static bool CheckIfLineHasSchedules(List<IElement> listOfHtmlSchedules)
            => listOfHtmlSchedules == null || listOfHtmlSchedules.Count() == 0;

        private static bool CheckIfScheduleIsFreezed(List<IElement> listOfHtmlSchedules)
            => listOfHtmlSchedules == null || listOfHtmlSchedules.Count() == 0;

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
            foreach (var element in collection)
            {
                type = GetLineSumBitFromType(element.FirstElementChild.ClassName);
                if ((type & (1 << 12)) == (1 << 12)) //if is 'koleje'
                    continue;

                Line line = new Line(true)
                {
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
    }
}
