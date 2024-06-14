﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MobileTelevision
{
    public interface IMobilePhoneApi
    {
        public event EventHandler<StardewModdingAPI.Events.RenderedWorldEventArgs> OnBeforeRenderScreen;
        public event EventHandler<StardewModdingAPI.Events.RenderedWorldEventArgs> OnAfterRenderScreen;

        bool AddApp(string id, string name, Action action, Texture2D icon);
        Vector2 GetRawScreenPosition();
        Vector2 GetRawScreenSize();
        Vector2 GetRawScreenSize(bool rotated);
        Rectangle GetRawPhoneRectangle();
        Rectangle GetRawScreenRectangle();
        Vector2 GetScreenPosition();
        Vector2 GetScreenSize();
        Vector2 GetScreenSize(bool rotated);
        Rectangle GetPhoneRectangle();
        Rectangle GetScreenRectangle();
        float GetUIScale();
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