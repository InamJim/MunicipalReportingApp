using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalReportingApp.Models;

namespace MunicipalReportingApp.Data
{

    public class EventRecommendationEngine
    {
        private readonly Dictionary<string, int> _categorySearchCounts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<EventSearchRecord> _recentSearches = new Stack<EventSearchRecord>();

        public int SearchCount { get { return _recentSearches.Count; } }

        public void RecordSearch ( string keyword, string category, bool usedDateRange,
            DateTime startDate, DateTime endDate, IList<LocalEvent> results )
        {
            IList<LocalEvent> safeResults = results ?? new List<LocalEvent>();
            if ( !string.IsNullOrWhiteSpace(category) &&
                !string.Equals(category, "All categories", StringComparison.OrdinalIgnoreCase) )
                AddCategoryWeight(category, 3);

            foreach ( string resultCategory in safeResults.Select(ev => ev.Category)
                .Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase) )
                AddCategoryWeight(resultCategory, 1);

            _recentSearches.Push(new EventSearchRecord
            {
                SearchedAt = DateTime.Now,
                Keyword = keyword ?? string.Empty,
                Category = category ?? "All categories",
                UsedDateRange = usedDateRange,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                ResultCount = safeResults.Count
            });
        }

        public List<EventSearchRecord> GetRecentSearches ( int count )
        {
            return _recentSearches.Take(Math.Max(0, count)).ToList();
        }

        public List<LocalEvent> GetRecommendations ( IEnumerable<LocalEvent> candidates,
            IEnumerable<int> excludedEventIds, int count )
        {
            HashSet<int> excluded = new HashSet<int>(excludedEventIds ?? Enumerable.Empty<int>());
            List<EventSearchRecord> history = _recentSearches.Take(12).ToList();
            List<string> keywords = history.SelectMany(record => SplitWords(record.Keyword))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            DateTime today = DateTime.Today;

            return (candidates ?? Enumerable.Empty<LocalEvent>())
                .Where(ev => ev != null && ev.EventDate >= DateTime.Now && !excluded.Contains(ev.Id))
                .Select(ev => new
                {
                    Event = ev,
                    Score = ScoreEvent(ev, history, keywords, today)
                })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Event.EventDate)
                .ThenBy(item => item.Event.Title)
                .Take(Math.Max(0, count))
                .Select(item => item.Event)
                .ToList();
        }

        private int ScoreEvent ( LocalEvent ev, IList<EventSearchRecord> history, IList<string> keywords, DateTime today )
        {
            int score = 0;
            if ( _categorySearchCounts.TryGetValue(ev.Category ?? string.Empty, out int affinity) )
                score += affinity * 8;

            string searchableText = string.Join(" ", ev.Title ?? string.Empty, ev.Description ?? string.Empty,
                ev.Location ?? string.Empty, ev.Category ?? string.Empty);
            foreach ( string word in keywords )
                if ( word.Length >= 3 && searchableText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 )
                    score += 5;

            foreach ( EventSearchRecord record in history )
            {
                if ( record.UsedDateRange && ev.EventDate.Date >= record.StartDate.Date && ev.EventDate.Date <= record.EndDate.Date )
                    score += 2;
            }

            int daysAway = Math.Max(0, (ev.EventDate.Date - today).Days);
            if ( daysAway <= 14 ) score += 3;
            if ( ev.IsAnnouncement ) score += 2;
            score += Math.Max(0, Math.Min(5, ev.Priority));
            return score;
        }

        private void AddCategoryWeight ( string category, int weight )
        {
            if ( _categorySearchCounts.ContainsKey(category) )
                _categorySearchCounts[category] += weight;
            else
                _categorySearchCounts[category] = weight;
        }

        private static IEnumerable<string> SplitWords ( string value )
        {
            if ( string.IsNullOrWhiteSpace(value) ) return Enumerable.Empty<string>();
            char[] separators = { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '/', '\\', '-', '_', '(', ')', '[', ']' };
            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim())
                .Where(word => word.Length > 0);
        }
    }
}
