﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ScottIsAFool.WindowsPhone.IsolatedStorage;
using StoreCardBuddy.Model;
using StoreCardBuddy.ViewModel;

namespace StoreCardBuddy
{
    public partial class App : Application
    {
        public static void ShowMessage(string message, string title = "", Action action = null)
        {
            var prompt = new ToastPrompt
            {
                Title = title,
                Message = message,
                Foreground = new SolidColorBrush(Colors.White),
            };

            if (action != null)
                prompt.Tap += (s, e) => action();
            prompt.Show();
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            FlurryWP7SDK.Api.StartSession("KP3C6B5C4WM6GPVJW95P");
            LoadCards();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            FlurryWP7SDK.Api.StartSession("KP3C6B5C4WM6GPVJW95P");
            if (!e.IsApplicationInstancePreserved)
            {
                LoadCards();
            }
        }

        private static void LoadCards()
        {
            if (ISettings.ContainsKey("TheCards"))
            {
                var cards = ISettings.GetKeyValue<ObservableCollection<Card>>("TheCards");
                SimpleIoc.Default.GetInstance<MainViewModel>().Cards = cards;
            }

            LoadSkyDriveSettings();
        }

        private static void LoadSkyDriveSettings()
        {
            var sri = GetResourceStream(new Uri("SkyDriveAccess.txt", UriKind.Relative));
            if (sri != null)
            {
                using (var reader = new StreamReader(sri.Stream))
                {
                    var clientId = reader.ReadToEnd();

                    Messenger.Default.Send(new NotificationMessage(clientId, "SkydriveDetails"));
                }
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveCards();
        }

        private static void SaveCards()
        {
            ISettings.Set("TheCards", SimpleIoc.Default.GetInstance<MainViewModel>().Cards);
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveCards();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame
                            {
                                Background = new SolidColorBrush(Colors.Transparent)
                            };
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}