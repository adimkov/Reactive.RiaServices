// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostDomainService.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2012. All rights reserved.
// </copyright>
// <summary>
//  Declaration of the Post domain service.
// </summary>
// -------------------------------------------------------------------------------------------------------------------
namespace Reactive.RiaServices.Samples.Web.Service
{
    using System;
    using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;

    using Reactive.RiaServices.Samples.Web.Model;

    /// <summary>
    /// Declaration of the Post domain service.
    /// </summary>
    [EnableClientAccess]
    public class PostDomainService : DomainService
    {
        public IQueryable<Post> LoadPosts()
        {
            throw new NotImplementedException();
        }

        [Insert]
        public void AddPost(Post post)
        {
            throw new NotImplementedException();   
        }

        [Update]
        public void UpdatePost(Post post)
        {
            throw new NotImplementedException();
        }

        [Delete]
        public void DeletePost(Post post)
        {
            throw new NotImplementedException();
        }

        [Invoke]
        public Post GetPredefinedPost()
        {
            throw new NotImplementedException();
        }
    }
}


