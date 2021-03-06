﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mastonet;
using Mastonet.Entities;
using Newtonsoft.Json;
using WinMasto.Models;
using WinMasto.Tools;
using WinMasto.Views;

namespace WinMasto.ViewModels
{
    public class StatusBaseViewModel : WinMastoViewModel
    {
        internal TimelineStreaming _timelineStreaming;

        public TimelineScrollingCollection Statuses { get; set; }

        public NotificationScrollingCollection Notifications { get; set; }

        public async Task ReplyOption(Status status)
        {
            await Template10.Common.BootStrapper.Current.NavigationService.NavigateAsync(typeof(NewStatusPage), new NewStatusParameter { IsReply = true, Status = status});
        }

        public async Task MentionOption(Status status)
        {
            await Template10.Common.BootStrapper.Current.NavigationService.NavigateAsync(typeof(NewStatusPage), new NewStatusParameter { IsMention = true, Status = status });
        }

        public async Task<Relationship> BlockOption(Status status)
        {
            try
            {
                return await Client.Block(status.Account.Id);
            }
            catch (Exception e)
            {
                await MessageDialogMaker.SendMessageDialogAsync(e.Message, false);
            }

            return new Relationship();
        }

        public async Task<Relationship> MuteOption(Status status)
        {
            try
            {
                return await Client.Mute(status.Account.Id);
            }
            catch (Exception e)
            {
                await MessageDialogMaker.SendMessageDialogAsync(e.Message, false);
            }

            return new Relationship();
        }

        public async Task ReportOption(Status status)
        {

        }

        public async Task ShowStatusOption(Status status)
        {
            await Template10.Common.BootStrapper.Current.NavigationService.NavigateAsync(typeof(StatusPage), status);
        }

        public async Task PullToRefresh()
        {
            await Statuses.PullToRefresh();
        }

        public async Task PullToRefreshNotifications()
        {
            await Notifications.PullToRefresh();
        }

        public async Task NavigateToAccountPage(Account account)
        {
            await Template10.Common.BootStrapper.Current.NavigationService.NavigateAsync(typeof(Views.AccountPage), JsonConvert.SerializeObject(account));
        }

        public async Task ShowNSFWPost(Status status)
        {
            Status newStatus = status;
            newStatus.Sensitive = false;
            newStatus.SpoilerText = string.Empty;
            if (Statuses != null)
            {
                var index = Statuses.IndexOf(status);
                Statuses[index] = newStatus;
            }
        }

        public async Task ShowNSFWPostNotifications(Notification notification)
        {
            Status newStatus = notification.Status;
            var index = Notifications.IndexOf(notification);
            newStatus.Sensitive = false;
            newStatus.SpoilerText = string.Empty;
            if (Notifications != null)
            {
                notification.Status = newStatus;
                Notifications[index] = notification;
            }
        }

        public async Task<Status> ReShareOption(Status status)
        {
            // TODO: This "works", but it could be more simple. The API layer needs to be tweeked.
            // It would make more sense to replace the status object in the list with the one it gets from the API
            // But it's not updating, because OnPropertyChanged is not in Status...
            // Reblog returns "reblogged" status, not the original status updated.
            try
            {
                Status newStatus = status;
                if (status.Reblogged == null)
                {
                    await Client.Reblog(status.Id);
                    newStatus.Reblogged = true;
                    newStatus.ReblogCount = newStatus.ReblogCount + 1;
                }
                else
                {
                    var reblogged = !status.Reblogged.Value;
                    if (reblogged)
                    {
                        await Client.Reblog(status.Id);
                        newStatus.Reblogged = true;
                        newStatus.ReblogCount = newStatus.ReblogCount + 1;
                    }
                    else
                    {
                        await Client.Unreblog(status.Id);
                        newStatus.Reblogged = false;
                        newStatus.ReblogCount = newStatus.ReblogCount - 1;
                    }
                }
                if (Statuses != null)
                {
                    var index = Statuses.IndexOf(status);
                    Statuses[index] = newStatus;
                }
                var notifications = Notifications?.Where(node => node.Status == status).ToList();
                if (notifications != null)
                {
                    for (var i = 0; i < notifications.Count(); i++)
                    {
                        var index = Notifications.IndexOf(notifications[i]);
                        var newNotification = notifications[i];
                        newNotification.Status = newStatus;
                        Notifications[index] = newNotification;
                    }
                }
                status = newStatus;
            }
            catch (Exception e)
            {
                await MessageDialogMaker.SendMessageDialogAsync(e.Message, false);
            }
            return status;
        }

        public async Task<Status> FavoriteOption(Status status)
        {
            // TODO: This "works", but it could be more simple. The API layer needs to be tweeked.
            // It would make more sense to replace the status object in the list with the one it gets from the API
            // But it's not updating, because OnPropertyChanged is not in Status...
            try
            {
                Status newStatus;
                if (status.Favourited == null)
                {
                    newStatus = await Client.Favourite(status.Id);
                }
                else
                {
                    var favorite = !status.Favourited.Value;
                    if (favorite)
                    {
                        newStatus = await Client.Favourite(status.Id);
                    }
                    else
                    {

                        // API Bug: Unfavorite returns a status that still says it's favorited, even though it's not.
                        // Not sure if it's mastodon, or the instance I'm on. So for now, we'll force it to say it's
                        // not there.
                        newStatus = await Client.Unfavourite(status.Id);
                        newStatus.Favourited = false;
                        newStatus.FavouritesCount = newStatus.FavouritesCount - 1;
                    }
                }
                if (Statuses != null)
                {
                    var index = Statuses.IndexOf(status);
                    Statuses[index] = newStatus;
                }
                var notifications = Notifications?.Where(node => node.Status == status).ToList();
                if (notifications != null)
                {
                    for (var i = 0; i < notifications.Count(); i++)
                    {
                        var index = Notifications.IndexOf(notifications[i]);
                        var newNotification = notifications[i];
                        newNotification.Status = newStatus;
                        Notifications[index] = newNotification;
                    }
                }
                status = newStatus;
            }
            catch (Exception e)
            {
                await MessageDialogMaker.SendMessageDialogAsync(e.Message, false);
            }
            return status;
        }

        public async Task NavigateToLoginView()
        {
            await Template10.Common.BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
        }

        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        internal string _path;

        internal string _accountname;
    }
}
