using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalReportingApp.Models;

namespace MunicipalReportingApp.Data
{

    public static class EventRepository
    {
        private static readonly SortedDictionary<DateTime, List<LocalEvent>> _eventsByDate =
            new SortedDictionary<DateTime, List<LocalEvent>>();
        private static readonly Dictionary<int, LocalEvent> _eventsById = new Dictionary<int, LocalEvent>();
        private static readonly Dictionary<string, List<LocalEvent>> _eventsByCategory =
            new Dictionary<string, List<LocalEvent>>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _uniqueCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<DateTime> _uniqueDates = new HashSet<DateTime>();
        private static readonly Queue<LocalEvent> _upcomingQueue = new Queue<LocalEvent>();
        private static readonly PriorityQueue<LocalEvent, (int Priority, long EventTicks)> _announcementPriorityQueue =
            new PriorityQueue<LocalEvent, (int Priority, long EventTicks)>();

        private static int _nextId = 1;
        private static bool _seeded;
        public static int Count { get { EnsureSeeded(); return _eventsById.Count; } }
        public static int CategoryCount { get { EnsureSeeded(); return _uniqueCategories.Count; } }
        public static int UniqueDateCount { get { EnsureSeeded(); return _uniqueDates.Count; } }
        public static IReadOnlyCollection<string> Categories { get { EnsureSeeded(); return _uniqueCategories.OrderBy(c => c).ToList().AsReadOnly(); } }

        static EventRepository ()
        {
            EnsureSeeded();
        }

        private static void EnsureSeeded ()
        {
            if ( _seeded ) return;
            _seeded = true;
            DateTime today = DateTime.Today;
            AddSeed("Community Clean-Up Morning", "Environment", today.AddDays(2).AddHours(9), "Riverside Park main entrance", "Join neighbours and local volunteers to clean public spaces. Gloves and rubbish bags will be supplied.", false, 2);
            AddSeed("Ward 4 Community Meeting", "Community Meetings", today.AddDays(4).AddHours(18), "Civic Centre, Hall A", "Residents can raise local priorities and hear updates from municipal representatives.", false, 2);
            AddSeed("Library Story Time", "Family and Youth", today.AddDays(5).AddHours(10), "Central Public Library", "A free reading and storytelling session for children and caregivers. Booking is not required.", false, 1);
            AddSeed("Water Network Maintenance Notice", "Utilities", today.AddDays(3).AddHours(7), "Northside service area", "Demonstration notice: scheduled water network maintenance may temporarily affect supply. Check official municipal channels for verified service updates.", true, 5);
            AddSeed("Weekend Local Market", "Markets and Culture", today.AddDays(7).AddHours(9), "Market Square", "Browse local produce, crafts, food stalls and small-business stands.", false, 1);
            AddSeed("Youth Sports Day", "Sports and Recreation", today.AddDays(9).AddHours(8), "Community Sports Ground", "Friendly games and skills activities for young people. Comfortable clothing and water are recommended.", false, 2);
            AddSeed("Public Holiday Waste Collection Reminder", "Waste Management", today.AddDays(6).AddHours(8), "Municipal collection routes", "Demonstration reminder: collection schedules may change around public holidays. Verify the route and date with the municipality.", true, 4);
            AddSeed("Parks Consultation Workshop", "Environment", today.AddDays(12).AddHours(17), "Greenview Recreation Centre", "Share ideas about park facilities, trees, accessibility and safer public areas.", false, 2);
            AddSeed("Small Business Information Session", "Business and Skills", today.AddDays(14).AddHours(14), "Enterprise Support Hub", "An introductory session about business support resources, permits and available training.", false, 1);
            AddSeed("Road Resurfacing Community Notice", "Roads and Transport", today.AddDays(1).AddHours(6), "Main Road near the civic precinct", "Demonstration notice: temporary lane restrictions may apply during scheduled resurfacing. Confirm dates and detours using official municipal updates.", true, 4);
            AddSeed("Arts in the Park", "Markets and Culture", today.AddDays(18).AddHours(11), "Botanical Gardens lawn", "An outdoor showcase with local artists, live performances and hands-on creative activities.", false, 1);
            AddSeed("Community Safety Forum", "Community Meetings", today.AddDays(21).AddHours(18), "Ward Office meeting room", "Residents are invited to discuss safety concerns, neighbourhood cooperation and prevention initiatives.", false, 3);
            RebuildUpcomingQueue();
        }

        private static void AddSeed ( string title, string category, DateTime date, string location, string description, bool announcement, int priority )
        {
            Add(new LocalEvent
            {
                Title = title,
                Category = category,
                EventDate = date,
                Location = location,
                Description = description,
                IsAnnouncement = announcement,
                Priority = priority
            });
        }
        public static LocalEvent Add ( LocalEvent localEvent )
        {
            if ( localEvent == null ) throw new ArgumentNullException(nameof(localEvent));
            if ( string.IsNullOrWhiteSpace(localEvent.Title) ) throw new ArgumentException("An event title is required.", nameof(localEvent));
            if ( string.IsNullOrWhiteSpace(localEvent.Category) ) throw new ArgumentException("An event category is required.", nameof(localEvent));
            if ( localEvent.Id <= 0 ) localEvent.Id = _nextId++;
            else _nextId = Math.Max(_nextId, localEvent.Id + 1);
            localEvent.Priority = Math.Max(1, Math.Min(5, localEvent.Priority));

            _eventsById[localEvent.Id] = localEvent;
            DateTime dateKey = localEvent.EventDate.Date;
            if ( !_eventsByDate.TryGetValue(dateKey, out List<LocalEvent> onDate) )
            {
                onDate = new List<LocalEvent>();
                _eventsByDate[dateKey] = onDate;
            }
            onDate.Add(localEvent);
            _uniqueDates.Add(dateKey);
            _uniqueCategories.Add(localEvent.Category);
            if ( !_eventsByCategory.TryGetValue(localEvent.Category, out List<LocalEvent> inCategory) )
            {
                inCategory = new List<LocalEvent>();
                _eventsByCategory[localEvent.Category] = inCategory;
            }
            inCategory.Add(localEvent);
            if ( localEvent.IsAnnouncement )
                _announcementPriorityQueue.Enqueue(localEvent, (-localEvent.Priority, localEvent.EventDate.Ticks));
            RebuildUpcomingQueue();
            return localEvent;
        }

        public static LocalEvent GetById ( int id )
        {
            EnsureSeeded();
            _eventsById.TryGetValue(id, out LocalEvent found);
            return found;
        }

        public static List<LocalEvent> GetUpcomingEvents ( DateTime? startDate = null, DateTime? endDate = null )
        {
            EnsureSeeded();
            DateTime start = (startDate ?? DateTime.Today).Date;
            DateTime end = (endDate ?? DateTime.MaxValue).Date;
            if ( end < start ) return new List<LocalEvent>();

            return _eventsByDate
                .Where(pair => pair.Key >= start && pair.Key <= end)
                .SelectMany(pair => pair.Value)
                .Where(ev => ev.EventDate >= DateTime.Now)
                .OrderBy(ev => ev.EventDate)
                .ThenBy(ev => ev.Title)
                .ToList();
        }

        public static List<LocalEvent> Search ( string keyword, string category, DateTime? startDate, DateTime? endDate )
        {
            EnsureSeeded();
            IEnumerable<LocalEvent> candidates;
            if ( !string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase)
                && _eventsByCategory.TryGetValue(category, out List<LocalEvent> categoryEvents) )
                candidates = categoryEvents;
            else
                candidates = _eventsById.Values;

            string term = (keyword ?? string.Empty).Trim();
            DateTime start = (startDate ?? DateTime.Today).Date;
            DateTime end = (endDate ?? DateTime.MaxValue).Date;
            if ( end < start ) return new List<LocalEvent>();

            return candidates
                .Where(ev => ev.EventDate.Date >= start && ev.EventDate.Date <= end)
                .Where(ev => ev.EventDate >= DateTime.Now)
                .Where(ev => term.Length == 0 || ContainsText(ev.Title, term) || ContainsText(ev.Description, term)
                    || ContainsText(ev.Location, term) || ContainsText(ev.Category, term))
                .OrderBy(ev => ev.EventDate)
                .ThenBy(ev => ev.Title)
                .ToList();
        }

        public static List<LocalEvent> GetPriorityAnnouncements ( int count )
        {
            EnsureSeeded();
            int requested = Math.Max(0, count);
            var result = new List<LocalEvent>();
            var restore = new List<LocalEvent>();

            while ( result.Count < requested && _announcementPriorityQueue.Count > 0 )
            {
                if ( !_announcementPriorityQueue.TryDequeue(out LocalEvent next, out var priority) )
                    break;
                if ( next.EventDate >= DateTime.Now )
                {
                    result.Add(next);
                    restore.Add(next);
                }
            }
            foreach ( LocalEvent localEvent in restore )
                _announcementPriorityQueue.Enqueue(localEvent, (-localEvent.Priority, localEvent.EventDate.Ticks));
            return result;
        }
        public static List<LocalEvent> GetUpcomingQueueSnapshot ()
        {
            EnsureSeeded();
            return _upcomingQueue.Where(ev => ev.EventDate >= DateTime.Now).ToList();
        }

        private static bool ContainsText ( string source, string term )
        {
            return (source ?? string.Empty).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void RebuildUpcomingQueue ()
        {
            _upcomingQueue.Clear();
            foreach ( LocalEvent ev in _eventsByDate.SelectMany(pair => pair.Value)
                .Where(ev => ev.EventDate >= DateTime.Now).OrderBy(ev => ev.EventDate) )
                _upcomingQueue.Enqueue(ev);
        }
    }
}
