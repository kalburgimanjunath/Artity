﻿namespace Artity.Data.Models
{
    using System;

    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Artity.Data.Common.Models;

    public class Artist : BaseModel<string>, IDeletableEntity
    {
        public Artist()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Offerts = new List<Offert>();
        }

        [Required]
        [MaxLength(30)]
        public string Nikname { get; set; }

        public string WorkNumber { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(2200)]
        public string AboutMe { get; set; }

        public string CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public string ProfilePictureId { get; set; }

        public virtual Picture ProfilePicture { get; set; }

        public virtual IList<Offert> Offerts { get; set; }
    
        public virtual IList<Performence> Performences { get; set; }

        public bool IsDeleted { get; set; }

        public string SocialId { get; set; }

        public virtual Social Social { get; set; }

        public DateTime? DeletedOn { get; set; }

        [Required]
        public bool IsApproved { get; set; }
    }
}