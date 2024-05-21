﻿namespace PersonalSiteBackend.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public List<string> Sources { get; set; }
    }
}