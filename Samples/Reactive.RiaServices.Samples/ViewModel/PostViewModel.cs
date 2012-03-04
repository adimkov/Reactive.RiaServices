// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostViewModel.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2012. All rights reserved.
// </copyright>
// <summary>
//  Declaration of the PostViewModel class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Reactive.RiaServices.Samples.ViewModel
{
    using System;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.ViewModel;

    using Reactive.RiaServices.Samples.Web.Service;

    /// <summary>
    /// Declaration of the PostViewModel class.
    /// </summary>
    public class PostViewModel : NotificationObject
    {
        /// <summary>
        /// The domain context.
        /// </summary>
        private readonly PostDomainContext _postDomainContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostViewModel"/> class.
        /// </summary>
        public PostViewModel()
        {
            _postDomainContext = new PostDomainContext();
            LoadPostsCommand = new DelegateCommand(LoadPostCommandExecute);
            SubmitContextCommand = new DelegateCommand(SubmitContextCommandExecute);
            GetPredefinedPostCommand = new DelegateCommand(GetPredefinedPostCommandExecute);
        }

        /// <summary>
        /// Gets the load posts.
        /// </summary>
        public ICommand LoadPostsCommand { get; private set; }

        /// <summary>
        /// Gets the submit context command.
        /// </summary>
        public ICommand SubmitContextCommand { get; private set; }

        /// <summary>
        /// Gets the get predefined post command.
        /// </summary>
        public ICommand GetPredefinedPostCommand { get; private set; }

        /// <summary>
        /// Gets the post context.
        /// </summary>
        public PostDomainContext PostContext
        {
            get
            {
                return _postDomainContext;
            }
        }

        /// <summary>
        /// Executes command of load posts.
        /// </summary>
        private void LoadPostCommandExecute()
        {
            // Loads the posts in context
            PostContext.LoadPostsQuery()
                .ToObservable(PostContext)
                .Subscribe(
                x => { }, 
                ex => { });
        }

        /// <summary>
        /// Submits the context.
        /// </summary>
        private void SubmitContextCommandExecute()
        {
            // Submit context on server
            PostContext.GetObservableSubmitChanges()
                .SilentSubscribe();
        }

        /// <summary>
        /// Executes the command to get the predefined post.
        /// </summary>
        private void GetPredefinedPostCommandExecute()
        {
            PostContext.GetObservableInvoke(x => x.GetPredefinedPost())
                .Subscribe(
                post =>
                    {
                        // handle result
                    });
        }
    }
}