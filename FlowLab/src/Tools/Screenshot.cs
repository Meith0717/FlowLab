// Screenshot.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.IO;

namespace FlowLab.Tools;

public class Screenshot(PathService<AppPaths> pathService)
{
    private bool _take;

    public void TakeScreenshot()
    {
        _take = true;
    }

    public void Update(RenderTarget2D renderTarget2D)
    {
        if (!_take)
            return;

        var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        var filePath = pathService.GetFilePath(AppPaths.Screenshots, timestamp + ".png");

        Task.Run(() =>
        {
            using var stream = File.Create(filePath);
            renderTarget2D.SaveAsPng(stream, renderTarget2D.Width, renderTarget2D.Height);
        });

        _take = false;
    }
}
