// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Post.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2012. All rights reserved.
// </copyright>
// <summary>
//  Declaration of the Post model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Reactive.RiaServices.Samples.Web.Model
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Declaration of the Post model.
    /// </summary>
    public class Post
    {
        /// <summary>
        /// Gets identifier of the post.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [Key]
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }
    }
}