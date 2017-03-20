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
    public class TimetableDownloadService : Timetable
    {
        private const string _TIMETABLE_URL = "http://rozklady.kzkgop.pl/index.php?co=rozklady";
        private const string _TIMETABLE_BASE_URL = "http://rozklady.kzkgop.pl/";

        private TimetableDownloadService() { }

        private static int _LineId;
        private static int _ScheduleId;
        private static int _TrackId;
        private static int _StopId;
        private static int _HourId;
        private static int _LetterId;
        private static int _TrackNameId;
        private static int _StopNameId;
        private static int _HourNameId;
        private static int _LetterNameId;

        private static void ResetIdentyficators()
        {
            Timetable.Instance = new Timetable()
            {
                BusStopsNames = new List<BusStopName>(),
                TracksNames = new List<TrackName>(),
                HoursNames = new List<HourName>(),
                Letters = new List<Letter>(),
                LettersNames = new List<LetterName>()
            };
            _LineId = 0;
            _ScheduleId = 0;
            _TrackId = 0;
            _StopId = 0;
            _HourId = 0;
            _LetterId = 0;
            _TrackNameId = 0;
            _StopNameId = 0;
            _HourNameId = 0;
            _LetterNameId = 0;
        }

        public static async Task<bool> DownloadNewTimetableAsync()
        {
            ResetIdentyficators();
            List<Line> lines = await GetInfoAboutLinesAsync();
            bool areLinesDownloadedCorrectly = !(lines == null || lines.Count() == 0);
            if (areLinesDownloadedCorrectly)
            {
                Timetable.Instance.Lines = lines;
                InvokeOnAllLinesDownloaded();
            }
            return areLinesDownloadedCorrectly;
        }

        private static async Task<List<Line>> GetInfoAboutLinesAsync()
        {
            using (var htmlDocument = await GetAngleSharpDocumentOfSiteAsync(_TIMETABLE_URL))
            {
                if (htmlDocument == null)
                    return null;
                List<Line> lines = GetLinesInfoFromAngleSharpDocument(htmlDocument);
                if (lines == null || lines.Count() == 0)
                    return null;
                InvokeOnLinesInfoDownloaded();
                lines = await GetAllLinesDetailFromLinesInfoAsync(lines);
                if (lines == null || lines.Count() == 0)
                    return null;
                return lines;
            }
        }

        private static async Task<List<Line>> GetAllLinesDetailFromLinesInfoAsync(List<Line> lines)
        {
            int countOfLines = lines.Count();
            for (int i = 0; i < countOfLines; i++)
            {
                Line line = lines[i];
                line = await GetLineDetailInfoAsync(line);
                if (line == null)
                    return null;
                lines[i] = line;
                InvokeOnLineDownloaded(line, countOfLines);
            }
            return lines;
        }

        private static async Task<Line> GetLineDetailInfoAsync(Line line)
        {
            string url = string.Format("{0}{1}", _TIMETABLE_BASE_URL, line.Url);
            using (var lineAngSharpDocument = (await GetAngleSharpDocumentOfSiteAsync(url)))
            {
                if (lineAngSharpDocument == null)
                    return null;
                var lineHtmlElement = lineAngSharpDocument.Children.Count() >= 1 ? lineAngSharpDocument.Children[0] : null;
                if (lineHtmlElement == null)
                    return null;
                line = await AddSchedulesToLineAsync(line, lineHtmlElement, url);
                return line;
            }
        }

        private static async Task<Line> GetSchedulesDetailInfoAsync(Line line)
        {
            int countOfSchedules = line.Schedules.Count();
            for (int i = 0; i < countOfSchedules; i++)
            {
                Schedule schedule = line.Schedules[i];
                var htmlDocument = await GetAngleSharpDocumentOfSiteAsync(schedule.Url);
                if (htmlDocument == null)
                    return null;
                List<Track> listOfScheduleTracks = await GetTracksOfScheduleAngleSharpDocumentAsync(htmlDocument,
                    line.Id, schedule.Id);
                if (listOfScheduleTracks == null)
                    return null;
                schedule.Tracks = listOfScheduleTracks;
            }
            return line;
        }

        private static async Task<List<Track>> GetTracksOfScheduleAngleSharpDocumentAsync(IHtmlDocument htmlDocument, int lineId, int scheduleId)
        {
            var listOfHtmlTracks = htmlDocument.QuerySelectorAll("div")
                                    .Where(p => p.Id == "lewo" || p.Id == "srodek" || p.Id == "prawo");
            if (IsNotValidList(listOfHtmlTracks))
                return null;
            List<Track> listOfTracks = new List<Track>();
            foreach (var htmlTrack in listOfHtmlTracks)
            {
                Track track = new Track()
                {
                    Id = _TrackId++,
                    IdOfLine = lineId,
                    IdOfSchedule = scheduleId
                };
                track = await GetTrackDetailFromHtmlAsync(htmlTrack, track);
                if (track == null)
                    return null; ;
                listOfTracks.Add(track);
            }
            return listOfTracks;
        }

        private static async Task<Track> GetTrackDetailFromHtmlAsync(IElement htmlTrack, Track track)
        {
            string linkToFirstStop = htmlTrack.QuerySelectorAll("tr td a").First().GetAttribute("href");
            if (string.IsNullOrEmpty(linkToFirstStop))
                return null;
            string trackUrl = string.Format("{0}{1}", _TIMETABLE_BASE_URL, linkToFirstStop);
            track.Url = trackUrl;
            using (var htmlDocument = await GetAngleSharpDocumentOfSiteAsync(trackUrl))
            {
                if (htmlDocument == null)
                    return null;
                IEnumerable<IElement> tableOfHtmlStops = htmlDocument.QuerySelectorAll("table")
                                        .Where(p => p.Id == "div_trasy_table");
                if (IsNotValidList(tableOfHtmlStops))
                    return null;
                track = await GetTrackDetailByTableOfHtmlStopsAsync(tableOfHtmlStops, track);
                return track;
            }
        }

        private static async Task<Track> GetTrackDetailByTableOfHtmlStopsAsync(IEnumerable<IElement> htmlTableWithStops, Track track)
        {
            var htmlTrack = htmlTableWithStops.First().QuerySelectorAll("tr");
            if (IsNotValidHtmlList(htmlTrack))
                return null;
            string trackName = htmlTrack.First(p => p.ClassName == "tr_kierunek")
                                .FirstElementChild.TextContent.Remove(0, 10).Trim();
            if (string.IsNullOrEmpty(trackName))
                return null;
            track = SetTrackName(track, trackName);
            track = await GetTrackStopsFromHtmlAsync(track, htmlTrack);
            return track;
        }

        private static async Task<Track> GetTrackStopsFromHtmlAsync(Track track, IHtmlCollection<IElement> stopsRawHtmlCollection)
        {
            var stopsHtmlCollection = stopsRawHtmlCollection.Where(p => p.ClassName.Contains("zwyk") ||
                                    p.ClassName.Contains("stre") || p.ClassName.Contains("wyj"));
            track.BusStops = new List<BusStop>();
            foreach (var htmlRawStop in stopsHtmlCollection)
            {
                var htmlStop = htmlRawStop.LastElementChild;
                if (htmlStop == null)
                    return null;
                string busHtmlContentStyle = htmlStop.GetAttribute("style");
                BusStop busStop = new BusStop()
                {
                    Id = _StopId++,
                    IdOfLine = track.IdOfLine,
                    IdOfSchedule = track.IdOfSchedule,
                    IdOfTrack = track.Id,
                    Url = htmlStop.LastElementChild.GetAttribute("href"),
                    IsVariant = htmlStop.ClassName.Contains("wariant"),
                    IsLastStopOnTrack = (busHtmlContentStyle == null ? false : busHtmlContentStyle.Contains("bold")),
                    IsBusStopZone = htmlRawStop.ClassName.Contains("stref")
                };
                string busStopName = htmlStop.LastElementChild.TextContent;
                busStop = SetBusStopName(busStop, busStopName);
                busStop = await GetStopHoursAsync(track, busStop);
                if (busStop == null)
                    return null;
                track.BusStops.Add(busStop);
            }
            return track;
        }

        private static async Task<BusStop> GetStopHoursAsync(Track track, BusStop busStop)
        {
            using (var htmlDocument = await GetStopAngleSharpDocumentAsync(busStop))
            {
                if (htmlDocument == null)
                    return null;
                var listOfHtmlHours = htmlDocument.QuerySelectorAll("table")
                                        .Where(p => p.Id == "tabliczka_przystankowo");
                if (!AreNotHoursInHtmlStop(listOfHtmlHours))
                {
                    listOfHtmlHours = listOfHtmlHours.First().QuerySelectorAll("tr");
                    List<Hour> hours = GetHoursListFromHtmlList(htmlDocument, listOfHtmlHours, busStop);
                    if (hours == null)
                        return null;
                    busStop.Hours = hours;
                }
                else if (listOfHtmlHours == null)
                    return null;
                else
                    busStop.Hours = new List<Hour>();
                return busStop;
            }
        }

        private static List<Hour> GetHoursListFromHtmlList(IHtmlDocument htmlDocument, IEnumerable<IElement> listOfHtmlHours, BusStop busStom)
        {
            var listOfDaysType = listOfHtmlHours.Where((p, i) => i % 2 == 0);
            var listWithHoursOfDaysType = listOfHtmlHours.Where((p, i) => i % 2 == 1);
            List<Hour> listOfHours = new List<Hour>();
            int daysTypeCount = listOfDaysType.Count();
            for(int i = 0; i < daysTypeCount; i++)
            {
                var htmlDayType = listOfDaysType.ElementAt(i);
                var hour = new Hour()
                {
                    Id = _HourId++,
                    IdOfBusStop = busStom.Id
                };
                string hourName = htmlDayType.FirstElementChild.TextContent;
                hour = SetHourName(hour, hourName);
                hour = GetHoursFromDayType(hour, listWithHoursOfDaysType.ElementAt(i), htmlDocument);
                if (hour == null)
                    return null;
                listOfHours.Add(hour);
                
            }
            return listOfHours;
        }

        private static Hour GetHoursFromDayType(Hour hour, IElement htmlDayType, IHtmlDocument htmlDocument)
        {
            var listOfHtmlHours = htmlDayType.QuerySelectorAll("span")
                                    .Where(p => p.Id == "blok_godzina");
            if (IsNotValidList(listOfHtmlHours))
                return null;
            StringBuilder hoursString = new StringBuilder();
            foreach(var htmlHour in listOfHtmlHours)
            {
                string hourHours = GetHoursFromHtmlHour(htmlHour, htmlDocument);
                if (hourHours == null)
                    return null;
                hoursString.Append(hourHours);         
            }
            hour.Hours = hoursString.ToString();
            hour.Hours = hour.Hours.Remove(hour.Hours.Length - 1, 1); // remove last space
            return hour;
        }

        private static string GetHoursFromHtmlHour(IElement htmlHour, IHtmlDocument htmlDocument)
        {
            string hourPart, minutePart;
            hourPart = htmlHour.FirstElementChild.TextContent;
            var listOfHtmlMinutes = htmlHour.QuerySelectorAll("a");
            if (IsNotValidList(listOfHtmlMinutes))
                return null;
            StringBuilder hoursString = new StringBuilder();
            bool isLetter = false;
            foreach (var htmlMinute in listOfHtmlMinutes)
            {
                string letter = "";
                minutePart = GetMinutePartFromHtmlMinute(htmlMinute);
                if (minutePart == null)
                    return null;
                bool isLetterInHtmlMinute = htmlMinute.FirstElementChild.FirstElementChild != null;
                if(isLetterInHtmlMinute)
                {
                    letter = htmlMinute.FirstElementChild.FirstElementChild.TextContent;
                    minutePart = minutePart.Replace(letter, "");
                    isLetter = true;
                }
                hoursString.AppendFormat("{0}:{1}{2} ", hourPart, minutePart, letter);
            }
            if (isLetter)
                GetBusStopLetters(htmlDocument);
            return hoursString.ToString();
        }

        private static string GetMinutePartFromHtmlMinute(IElement htmlMinute)
        {
            try
            {
                return htmlMinute.FirstElementChild.TextContent;
            }
            catch
            {
                return null;
            }
        }

        private static void GetBusStopLetters(IHtmlDocument htmlDocument)
        {
            var htmlTableWithLetters = htmlDocument.QuerySelectorAll("table").FirstOrDefault(p => p.ClassName == "legenda_literki");
            if (htmlTableWithLetters == null)
                return;
            var listOfHtmlLetters = htmlTableWithLetters.QuerySelectorAll("tr");
            foreach(var htmlLetter in listOfHtmlLetters)
            {
                Char letterChar = htmlLetter.TextContent[0];
                var letter = new Letter()
                {
                    Id = _LetterId++,
                    IdOfBusStop = _StopId - 1
                };
                var letterName = htmlLetter.TextContent.Remove(0, 1).Insert(0, string.Format("{0} - ", letterChar));
                letter = SetLetterName(letter, letterName);
                Timetable.Instance.Letters.Add(letter);
            }
        }

        private static Letter SetLetterName(Letter letter, string letterName)
        {
            int letterNameId = -1;
            foreach (var letterNameClass in Timetable.Instance.LettersNames)
            {
                if (letterName == letterNameClass.Name)
                {
                    letter.IdOfName = letterNameId = letterNameClass.Id;
                    break;
                }
            }
            if (letterNameId == -1)
            {
                Timetable.Instance.LettersNames.Add(new LetterName()
                {
                    Id = _LetterNameId++,
                    Name = letterName
                });
                letter.IdOfName = _HourNameId - 1;
            }
            return letter;
        }

        private static Hour SetHourName(Hour hour, string hourName)
        {
            int hourNameId = -1;
            foreach (var hourNameClass in Timetable.Instance.HoursNames)
            {
                if (hourName == hourNameClass.Name)
                {
                    hour.IdOfName = hourNameId = hourNameClass.Id;
                    break;
                }
            }
            if (hourNameId == -1)
            {
                Timetable.Instance.HoursNames.Add(new HourName()
                {
                    Id = _HourNameId++,
                    Name = hourName
                });
                hour.IdOfName = _HourNameId - 1;
            }
            return hour;
        }

        private static async Task<IHtmlDocument> GetStopAngleSharpDocumentAsync(BusStop busStop)
        {
            string urlOfBusStop = string.Format("{0}{1}", _TIMETABLE_BASE_URL, busStop.Url);
            var document = await GetAngleSharpDocumentOfSiteAsync(urlOfBusStop);
            return document;
        }

        private static BusStop SetBusStopName(BusStop busStop, string busStopName)
        {
            int stopNameId = -1;
            foreach (var stopNameClass in Timetable.Instance.BusStopsNames)
            {
                if (busStopName == stopNameClass.Name)
                {
                    busStop.IdOfName = stopNameId = stopNameClass.Id;
                    break;
                }
            }
            if (stopNameId == -1)
            {
                Timetable.Instance.BusStopsNames.Add(new BusStopName()
                {
                    Id = _StopNameId++,
                    Name = busStopName
                });
                busStop.IdOfName = _StopNameId - 1;
            }
            return busStop;
        }

        private static Track SetTrackName(Track track, string trackName)
        {
            int trackNameId = -1;
            foreach (var trackNameClass in Timetable.Instance.TracksNames)
            {
                if (trackNameClass.Name == trackName)
                {
                    track.IdOfName = trackNameId = trackNameClass.Id;
                    break;
                }
            }
            if (trackNameId == -1)
            {
                Timetable.Instance.TracksNames.Add(new TrackName()
                {
                    Id = _TrackNameId++,
                    Name = trackName
                });
                track.IdOfName = _TrackNameId - 1;
            }
            return track;
        }

        private static async Task<Line> AddSchedulesToLineAsync(Line line, IElement htmlDocument, string url)
        {
            List<IElement> listOfHtmlSchedules = htmlDocument.QuerySelectorAll("td")
                      .Where(p => p.ClassName != null && p.ClassName.Contains("kier")).ToList();
            if (IsSchedulesInLineOrScheduleIsFreezed(listOfHtmlSchedules))
            {
                listOfHtmlSchedules = htmlDocument.QuerySelectorAll("div").Where(p => p.Id == "div_tabelki_tras").ToList();
                if (IsSchedulesInLineOrScheduleIsFreezed(listOfHtmlSchedules))
                {
                    line.Schedules = new List<Schedule>().Add<Schedule>(
                                        new Schedule() { Id = _ScheduleId++, Name = "linia zawieszona" });
                    return line;
                }
                GetSchedulesDetail(ref line, listOfHtmlSchedules);
            }
            else
                line.Schedules = new List<Schedule>().Add<Schedule>(
                                new Schedule()
                                {
                                    Id = _ScheduleId++,
                                    Name = "obecnie obowiązujący",
                                    IdOfLine = line.Id,
                                    Url = url,
                                    IsActualSchedule = true
                                });
            line = await GetSchedulesDetailInfoAsync(line);
            return line;
        }

        private static void GetSchedulesDetail(ref Line line, IEnumerable<IElement> listOfHtmlSchedules)
        {
            List<Schedule> listOfSchedules = new List<Schedule>();
            var schedules = listOfHtmlSchedules.ElementAt(0).Children[1].QuerySelectorAll("li a");
            foreach (var schedule in schedules)
            {
                listOfSchedules.Add(new Schedule()
                {
                    Id = _ScheduleId++,
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
            foreach (var element in collection)
            {
                type = GetLineSumBitFromType(element.FirstElementChild.ClassName);
                if ((type & Line.TRAIN_BIT) == Line.TRAIN_BIT) //if is 'koleje'
                    continue;
                Line line = new Line()
                {
                    Id = _LineId++,
                    Url = @element.GetAttribute("href"),
                    Name = element.FirstElementChild.TextContent,
                    Type = type
                };
                listOfLines.Add(line);
            }
            return listOfLines;
        }

        private static async Task<IHtmlDocument> GetAngleSharpDocumentOfSiteAsync(string url)
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
                bit |= Line.NORMAL_BUS_BIT;
            if (type.Contains("przysp"))
                bit |= Line.FAST_BUS_BIT;
            if (type.Contains("tram"))
                bit |= Line.TRAM_BIT;
            if (type.Contains("mini"))
                bit |= Line.MINI_BIT;
            if (type.Contains("lot"))
                bit |= Line.AIRPORT_BIT;
            if (type.Contains("swiezy"))
                bit |= Line.UPDATED_BIT;
            if (type.Contains("zast"))
                bit |= Line.REPLACMENT_BIT;
            if (type.Contains("wiekszy"))
                bit |= Line.BIG_BUS_BIT;
            if (type.Contains("nocna"))
                bit |= Line.NIGHT_BUS_BIT;
            if (type.Contains("bezp")) //no-paid, free
                bit |= Line.FREE_BIT;
            if (type.Contains("kolej"))
                bit |= Line.TRAIN_BIT;
            return bit;
        }

        private static bool AreNotHoursInHtmlStop(IEnumerable<IElement> listOfHtmlHours)
            => listOfHtmlHours == null || listOfHtmlHours.Count() == 0;

        private static bool IsStopFirstStopInTrack(Track track, BusStop busStop)
            => track.Name != busStop.Name;

        private static bool IsNotValidHtmlList(IHtmlCollection<IElement> htmlCollection)
            => htmlCollection == null || htmlCollection.Count() == 0;

        private static bool IsNotValidList(IEnumerable<IElement> htmlList)
            => htmlList == null || htmlList.Count() == 0;

        private static bool IsSchedulesInLineOrScheduleIsFreezed(List<IElement> listOfHtmlSchedules)
            => listOfHtmlSchedules == null || listOfHtmlSchedules.Count() == 0;
    }
}
