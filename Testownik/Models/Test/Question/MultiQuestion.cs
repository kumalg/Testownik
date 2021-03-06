﻿using System.Collections.Generic;

namespace Testownik.Models.Test {
    public class MultiQuestion : IQuestion {
        public IContent Content { get; set; }
        public List<List<IAnswer>> Answers { get; set; }
        public IList<string> CorrectAnswerKeys { get; set; }
    }
}