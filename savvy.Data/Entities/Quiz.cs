﻿using System.Collections.Generic;
using savvy.Data.Entities.Questions;

namespace savvy.Data.Entities
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}