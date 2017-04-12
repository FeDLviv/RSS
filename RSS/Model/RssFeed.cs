using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;        
using System.ServiceModel.Syndication;

namespace RSS.Model
{
    class RssFeed
    {
        private static readonly Regex regex = new Regex(@"<.+?>");

        public string Title { get; set; }
        public Uri Url { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }

        public RssFeed(SyndicationItem item)
        {
            Title = item.Title.Text; 
            Url = item.Links[0].Uri; 
            Date = item.PublishDate.ToString("HH:mm");
            Comment =  regex.Replace(item.Summary.Text, string.Empty);
        }
    }
}