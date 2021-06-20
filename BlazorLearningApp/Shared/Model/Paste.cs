using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorLearningApp.Shared.Model
{
    public class Paste
    {
        public int Expiration { get; set; } = 1;

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
