// Canvas.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;

namespace FlowLab.Game.Engine.UserInterface
{
    public enum Anchor { N, NE, E, SE, S, SW, W, NW, Center, CenterH, CenterV, Left, Right, Top, Down, None }
    public enum FillScale { X, Y, Both, FillIn, Fit, None }

    public class Canvas
    {
        // UiFrame
        private Rectangle _relativeBounds = new();
        private Rectangle _globalBounds = new();

        public Rectangle GetRelativeBounds() => _relativeBounds;

        public Rectangle GetGlobalBounds() => _globalBounds;

        public void UpdateFrame(Rectangle root, float uiScale, int? X = null, int? Y = null, int? Width = null, int? Height = null, float RelativeX = 0, float RelativeY = 0, float RelWidth = .1f, float RelHeight = .1f, int? HSpace = null, int? VSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            var x = (int)(X * uiScale ?? root.Width * RelativeX);
            var y = (int)(Y * uiScale ?? root.Height * RelativeY);

            var width = (int)(Width * uiScale ?? root.Width * RelWidth);
            var height = (int)(Height * uiScale ?? root.Height * RelHeight);

            ManageFillScale(root, fillScale, ref x, ref y, ref width, ref height);
            ManageAnchor(root, anchor, ref x, ref y, ref width, ref height);
            ManageSpacing(root, ref x, ref y, ref width, ref height, HSpace.HasValue ? HSpace * uiScale : null, VSpace.HasValue ? VSpace * uiScale : null);

            _relativeBounds.X = x;
            _relativeBounds.Y = y;
            _relativeBounds.Width = width;
            _relativeBounds.Height = height;

            _globalBounds.X = root.X + x;
            _globalBounds.Y = root.Y + y;
            _globalBounds.Width = width;
            _globalBounds.Height = height;
        }

        #region Apply Resolution and Position

        private void ManageFillScale(Rectangle root, FillScale fillScale, ref int x, ref int y, ref int width, ref int height)
        {
            var rootAspectRatio = root.Width / (float)root.Height;
            var aspectRatio = width / (float)height;

            switch (fillScale)
            {
                case FillScale.X:
                    width = root.Width;
                    height = (int)(width / aspectRatio);
                    break;
                case FillScale.Y:
                    height = root.Height;
                    width = (int)(height * aspectRatio);
                    break;
                case FillScale.Both:
                    x = 0; y = 0;
                    height = root.Height;
                    width = root.Width;
                    break;
                case FillScale.FillIn:
                    if (aspectRatio > rootAspectRatio)
                    {
                        height = root.Height;
                        width = (int)(height * aspectRatio);
                    }
                    if (aspectRatio < rootAspectRatio)
                    {
                        width = root.Width;
                        height = (int)(width / aspectRatio);
                    }
                    if (aspectRatio == rootAspectRatio)
                    {
                        x = 0; y = 0;
                        height = root.Height;
                        width = root.Width;
                    }
                    break;
                case FillScale.Fit:
                    if (aspectRatio > rootAspectRatio)
                    {
                        width = root.Width;
                        height = (int)(width / aspectRatio);
                    }
                    if (aspectRatio < rootAspectRatio)
                    {
                        height = root.Height;
                        width = (int)(height * aspectRatio);

                    }
                    if (aspectRatio == rootAspectRatio)
                    {
                        x = 0; y = 0;
                        height = root.Height;
                        width = root.Width;
                    }
                    break;
            }
        }

        private void ManageAnchor(Rectangle root, Anchor anchor, ref int x, ref int y, ref int width, ref int height)
        {
            x = anchor switch
            {
                Anchor.NW => 0,
                Anchor.SW => 0,
                Anchor.W => 0,
                Anchor.Left => 0,
                Anchor.N => (root.Width - width) / 2,
                Anchor.Center => (root.Width - width) / 2,
                Anchor.CenterV => (root.Width - width) / 2,
                Anchor.S => (root.Width - width) / 2,
                Anchor.NE => root.Width - width,
                Anchor.E => root.Width - width,
                Anchor.SE => root.Width - width,
                Anchor.Right => root.Width - width,
                Anchor.CenterH => x,
                Anchor.Top => x,
                Anchor.Down => x,
                Anchor.None => x,
                _ => throw new System.NotImplementedException()
            };
            y = anchor switch
            {
                Anchor.NW => 0,
                Anchor.N => 0,
                Anchor.NE => 0,
                Anchor.Top => 0,
                Anchor.E => (root.Height - height) / 2,
                Anchor.W => (root.Height - height) / 2,
                Anchor.Center => (root.Height - height) / 2,
                Anchor.CenterH => (root.Height - height) / 2,
                Anchor.SE => root.Height - height,
                Anchor.S => root.Height - height,
                Anchor.SW => root.Height - height,
                Anchor.Down => root.Height - height,
                Anchor.CenterV => y,
                Anchor.Left => y,
                Anchor.Right => y,
                Anchor.None => y,
                _ => throw new System.NotImplementedException()
            };
        }

        private void ManageSpacing(Rectangle root, ref int x, ref int y, ref int width, ref int height, float? hSpace, float? vSpace)
        {
            if (hSpace != null)
            {
                var spaceLeft = x;
                var spaceRight = root.Width - (width + x);

                if (spaceLeft < hSpace && spaceRight < hSpace)
                {
                    x += (int)hSpace;
                    width -= (int)hSpace * 2;
                }
                else
                {
                    if (spaceLeft < hSpace) x = 0 + (int)hSpace;
                    if (spaceRight < hSpace) x -= (int)hSpace;
                }
            }

            if (vSpace != null)
            {
                var spaceTop = y;
                var spaceBottom = root.Height - (height + y);

                if (spaceTop < vSpace && spaceBottom < vSpace)
                {
                    y += (int)vSpace;
                    height -= (int)vSpace * 2;
                }
                else
                {
                    if (spaceTop < vSpace) y = 0 + (int)vSpace;
                    if (spaceBottom < vSpace) y -= (int)vSpace;
                }
            }
        }

        private Vector2 mLastMousePosition;

        public void MouseDrag(InputState inputState)
        {
            Vector2 delta = inputState.MousePosition - mLastMousePosition;
            _relativeBounds.X += (int)delta.X;
            _relativeBounds.Y += (int)delta.Y;
            mLastMousePosition = inputState.MousePosition;
        }

        #endregion

        public void Draw(bool debug = true)
        {
            if (debug) return;
            TextureManager.Instance.DrawRectangleF(_relativeBounds, Color.Green, 1, 1);
        }
    }
}
