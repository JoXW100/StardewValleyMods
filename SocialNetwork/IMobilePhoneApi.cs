﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SocialNetwork
{
    public interface IMobilePhoneApi
    {
        event EventHandler<StardewModdingAPI.Events.RenderedWorldEventArgs> OnBeforeRenderScreen;
        event EventHandler<StardewModdingAPI.Events.RenderedWorldEventArgs> OnAfterRenderScreen;

        bool AddApp(string id, string name, Action action, Texture2D icon);
        Vector2 GetScreenPosition();
        Vector2 GetScreenSize();
        Vector2 GetScreenSize(bool rotated);
        Rectangle GetPhoneRectangle();
        Rectangle GetScreenRectangle();
        bool GetPhoneRotated();
        void SetPhoneRotated(bool value);
        bool GetPhoneOpened();
        void SetPhoneOpened(bool value);
        bool GetAppRunning();
        void SetAppRunning(bool value);
        string GetRunningApp();
        void SetRunningApp(string value);
    }
}