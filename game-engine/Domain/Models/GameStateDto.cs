﻿using System.Collections.Generic;

namespace Domain.Models
{
    public class GameStateDto
    {
        public World World { get; set; }
        public Dictionary<string, List<int>> GameObjects { get; set; }
        public Dictionary<string, List<int>> PlayerObjects { get; set; }
    }
}