using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Xaml.Behaviors;

namespace PunchClockIn.Behaviors;

public class TaskbarIconBalloonBehavior : Behavior<TaskbarIcon>
{
    public static readonly DependencyProperty ShowBalloonProperty;
    public static readonly DependencyProperty CustomBalloonTypeProperty;
    public static readonly DependencyProperty CustomBalloonAnimationProperty;
    public static readonly DependencyProperty BalloonTipTitleProperty;
    public static readonly DependencyProperty BalloonTipMessageProperty;

    static TaskbarIconBalloonBehavior()
    {
        ShowBalloonProperty = DependencyProperty.Register(
            nameof(ShowBalloon),
            typeof(bool),
            typeof(TaskbarIconBalloonBehavior),
            new PropertyMetadata(false, OnShowBalloonPropertyChanged));
        CustomBalloonTypeProperty = DependencyProperty.Register(
            nameof(CustomBalloonType),
            typeof(Type),
            typeof(TaskbarIconBalloonBehavior),
            new PropertyMetadata(null, OnCustomBalloonTypeChanged));
        CustomBalloonAnimationProperty = DependencyProperty.Register(
            nameof(CustomBalloonAnimation),
            typeof(PopupAnimation),
            typeof(TaskbarIconBalloonBehavior),
            new PropertyMetadata(PopupAnimation.Slide));
        BalloonTipTitleProperty = DependencyProperty.Register(
            nameof(BalloonTipTitle),
            typeof(string),
            typeof(TaskbarIconBalloonBehavior),
            new PropertyMetadata(null, null));
        BalloonTipMessageProperty = DependencyProperty.Register(
            nameof(BalloonTipMessage),
            typeof(string),
            typeof(TaskbarIconBalloonBehavior),
            new PropertyMetadata(null, null));
    }

    private static void OnCustomBalloonTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnShowBalloonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var show = (bool)e.NewValue;
        var behavior = d as TaskbarIconBalloonBehavior;
        var taskbarIcon = behavior.AssociatedObject;
        if(!show) 
            taskbarIcon.CloseBalloon();
        else
        {
            if (behavior.CustomBalloonType != null && typeof(Control).IsAssignableFrom(behavior.CustomBalloonType))
            {
                var control = behavior.CustomBalloonType.GetConstructor(Type.EmptyTypes)
                    .Invoke(new object[0]) as Control;
                taskbarIcon.ShowCustomBalloon(control, behavior.CustomBalloonAnimation, null);
            }
            else
                taskbarIcon.ShowBalloonTip("Title", "Message", BalloonIcon.Info);
        }
    }

    public bool ShowBalloon
    {
        get => (bool)GetValue(ShowBalloonProperty);
        set => SetValue(ShowBalloonProperty, value);
    }

    public Type CustomBalloonType
    {
        get => (Type)GetValue(CustomBalloonTypeProperty);
        set => SetValue(CustomBalloonTypeProperty, value);
    }
    public PopupAnimation CustomBalloonAnimation
    {
        get => (PopupAnimation)GetValue(CustomBalloonAnimationProperty);
        set => SetValue(CustomBalloonAnimationProperty, value);
    }

    public string BalloonTipTitle
    {
        get => (string)GetValue(BalloonTipTitleProperty);
        set => SetValue(BalloonTipTitleProperty, value);
    }
    public string BalloonTipMessage
    {
        get => (string)GetValue(BalloonTipMessageProperty);
        set => SetValue(BalloonTipMessageProperty, value);
    }
    protected override void OnAttached()
    {
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
    }
}