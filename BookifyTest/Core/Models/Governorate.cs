﻿namespace BookifyTest.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate : BaseModule
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
