﻿using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Pomodo7o
{
    public partial class Win
    {
        private readonly TaskbarManager _taskbarManager;
        private ThumbnailToolbarButton _btnReset;
        private ThumbnailToolbarButton _btnPlay;
        private ThumbnailToolbarButton _btnPause;

        private TomatoTimer _currentTimer;

        public Win(TaskbarManager taskbarManager)
        {
            _taskbarManager = taskbarManager;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workTimer = new TomatoTimer(5.Seconds(), 25.Minutes());
            var restTimer = new TomatoTimer(5.Seconds(), 5.Minutes());

            workTimer.TickPct += pct => Dispatch(() => UpdateProgress(pct));
            workTimer.TickRemaining += rmn => Dispatch(() => UpdateProgressState(GetProgressState(rmn)));
            workTimer.Complete += () =>
            {
                _currentTimer = restTimer;
                _currentTimer.Start();
                _taskbarManager.SetOverlayIcon(
                    Properties.Resources.icon_rest,
                    Properties.Resources.Mode_Rest);
            };

            restTimer.TickPct += pct => Dispatch(() => UpdateProgress(pct));
            restTimer.TickRemaining += rmn => Dispatch(() => UpdateProgressState(GetProgressStateRest(rmn)));
            restTimer.Complete += () =>
            {
                _currentTimer = workTimer;
                _currentTimer.Start();
                _taskbarManager.SetOverlayIcon(
                    null,
                    Properties.Resources.Mode_Work);
            };

            _currentTimer = workTimer;
            SetupThumbBar();
            _currentTimer.Start();
            UpdateThumbBar();
        }

        private void SetupThumbBar()
        {
            _btnReset = new ThumbnailToolbarButton(Properties.Resources.icon_reset, Properties.Resources.ToolTip_Reset).Chain(btn => btn.Click += (o, e) => ResetClicked());
            _btnPlay = new ThumbnailToolbarButton(Properties.Resources.icon_play, Properties.Resources.ToolTip_Play).Chain(btn => btn.Click += (o, e) => PlayClicked());
            _btnPause = new ThumbnailToolbarButton(Properties.Resources.icon_pause, Properties.Resources.ToolTip_Pause).Chain(btn => btn.Click += (o, e) => PauseClicked());

            _taskbarManager.ThumbnailToolbars.AddButtons(
                new WindowInteropHelper(this).Handle, _btnReset, _btnPlay, _btnPause);
        }

        private void PlayClicked()
        {
            _currentTimer.Start();
            UpdateThumbBar();
        }

        private void ResetClicked()
        {
            _currentTimer.Reset();
            UpdateThumbBar();
        }

        private void PauseClicked()
        {
            _currentTimer.Pause();
            UpdateThumbBar();
        }

        private void UpdateThumbBar()
        {
            _btnReset.Visible = true;
            _btnPlay.Visible = !_currentTimer.IsRunning;
            _btnPause.Visible = _currentTimer.IsRunning;
            _taskbarManager.SetOverlayIcon(
                _currentTimer.IsRunning ? null : Properties.Resources.icon_pause,
                _currentTimer.IsRunning ? String.Empty : Properties.Resources.Mode_Pause);
        }

        private void UpdateProgress(int percent)
        {
            _taskbarManager.SetProgressValue(percent, 100);
        }

        private void UpdateProgressState(TaskbarProgressBarState state)
        {
            _taskbarManager.SetProgressState(state);
        }

        private TaskbarProgressBarState GetProgressState(TimeSpan remaining)
        {
            if(remaining < 1.Minutes())
                return TaskbarProgressBarState.Error;

            if(remaining < 5.Minutes())
                return TaskbarProgressBarState.Paused;

            return TaskbarProgressBarState.Normal;
        }

        private TaskbarProgressBarState GetProgressStateRest(TimeSpan remaining)
        {
            if(remaining < 30.Seconds())
                return TaskbarProgressBarState.Error;

            if(remaining < 1.Minutes())
                return TaskbarProgressBarState.Paused;

            return TaskbarProgressBarState.Normal;
        }

        private void Dispatch(Action toDispatch)
        {
            Dispatcher.Invoke(toDispatch);
        }
    }
}
