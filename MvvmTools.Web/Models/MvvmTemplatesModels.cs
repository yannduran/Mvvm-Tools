﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace MvvmTools.Web.Models
{
    public class MvvmTemplateCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(60)]
        [Display(Name = "Category")]
        public string Name { get; set; }
    }

    /// <summary>
    /// A view and view model template.
    /// </summary>
    public class MvvmTemplate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User that owns this template.
        /// </summary>
        [Index("UK_ApplicationUserId_Name_Language", 0, IsUnique = true)]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [StringLength(100)]
        [Index("UK_ApplicationUserId_Name_Language", 1, IsUnique = true)]
        [Remote("NameAvailable", "Validation", ErrorMessage = "A template with that name/language combination already exists in your list.",
            AdditionalFields = "Language,Id")]
        public string Name { get; set; }

        /// <summary>
        /// 'VB' or 'C#'
        /// </summary>
        [Required]
        [StringLength(15)]
        [Index("UK_ApplicationUserId_Name_Language", 2, IsUnique = true)]
        [Display(Name = "Language")]
        [Remote("NameAvailable", "Validation", ErrorMessage = "A template with that name/language combination already exists in your list.",
            AdditionalFields = "Name,Id")]
        public string Language { get; set; }

        /// <summary>
        /// A single value.
        /// </summary>
        [Required]
        [Display(Name = "Category")]
        public int MvvmTemplateCategoryId { get; set; }
        public virtual MvvmTemplateCategory MvvmTemplateCategory { get; set; }

        /// <summary>
        /// A comma separated list for searching.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Tags (comma separated)")]
        public string Tags { get; set; }

        /// <summary>
        /// Text of the view model, with parameters inside.
        /// </summary>
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "View Model")]
        public string ViewModel { get; set; }

        /// <summary>
        /// Text of the view, with parameters inside.
        /// </summary>
        [Required]
        [DataType(DataType.MultilineText)]
        public string View { get; set; }

        /// <summary>
        /// If true, the template is shared.  Set to false while making modifications or to hide from others.
        /// </summary>
        [Required]
        public bool Enabled { get; set; }
    }
}