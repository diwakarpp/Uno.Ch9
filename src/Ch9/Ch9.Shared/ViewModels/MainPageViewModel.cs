﻿using System;
using Ch9.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace Ch9
{
	[Windows.UI.Xaml.Data.Bindable]
	public class MainPageViewModel : ViewModelBase
	{
		public MainPageViewModel()
		{
			ShowPost = new RelayCommand<PostViewModel>(post =>
			{
				if (SelectedPost != post)
				{
					SelectedPost = post;
				}
			});

			DismissPost = new RelayCommand(() => SelectedPost = null);

			ShowAboutPage = new RelayCommand(() =>
			{
				App.ServiceProvider.GetInstance<IStackNavigationService>().NavigateTo(nameof(AboutPage));
			});

			SharePost = new RelayCommand<PostViewModel>(async post =>
			{
				await Share.RequestAsync(new ShareTextRequest
				{
					Uri = post.Post.PostUri.ToString(),
					Title = post.Post.Title
				});
			});

			ReloadPage = new RelayCommand(LoadPosts);

            SendFeedBack = new RelayCommand(() => Launcher.OpenAsync(new Uri($"mailto:contact@unoplatform.com")));
        }

		public ICommand ShowPost { get; }

		public ICommand DismissPost { get; }

		public ICommand ShowAboutPage { get; }

		public ICommand ReloadPage { get; }

		public ICommand SharePost { get; }

		public ICommand SendFeedBack { get; }

		private PostViewModel _selectedPost;
		public PostViewModel SelectedPost
		{
			get => _selectedPost;
			set => Set(() => SelectedPost, ref _selectedPost, value);
		}

		private TaskNotifier<PostViewModel[]> _posts;
		public TaskNotifier<PostViewModel[]> Posts
		{
			get => _posts;
			set => Set(() => Posts, ref _posts, value);
		}

		private bool _isVideoFullWindow;
		public bool IsVideoFullWindow
		{
			get => _isVideoFullWindow;

			set
			{
				Set(() => IsVideoFullWindow, ref _isVideoFullWindow, value);

				App.OnFullscreenChanged(value);
			}
		}

		public void OnNavigatedTo()
		{
			// This method will be called when navigating back to the page.
			// Load the posts only when the posts are not set.
			if (Posts == null)
			{
				LoadPosts();
			}
		}

		private void LoadPosts()
		{
			async Task<PostViewModel[]> GetPosts()
			{
				var posts = await App.ServiceProvider.GetInstance<IPostsService>().GetRecentPosts();

				var postViewModels = posts.Select(p => new PostViewModel(this, p)).ToArray();

				return postViewModels;
			}

			Posts = new TaskNotifier<PostViewModel[]>(GetPosts());
		}
	}
}
