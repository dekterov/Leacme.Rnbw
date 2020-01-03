// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;
using System.Collections.Generic;

public class Hud : Node2D {

	private Color currentColor = Color.FromHsv(GD.Randf(), 1, 1);
	private List<(Vector2 pos, int size, Color color)> ptsToDraw = new List<(Vector2 pos, int size, Color color)>();

	private TextureRect vignette = new TextureRect() {
		Expand = true,
		Texture = new GradientTexture() {
			Gradient = new Gradient() { Colors = new[] { Colors.Transparent } }
		},
		Material = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type canvas_item;
					void fragment() {
						float iRad = 0.3;
						float oRad = 1.0;
						float opac = 0.5;
						vec2 uv = SCREEN_UV;
					    vec2 cent = uv - vec2(0.5);
					    vec4 tex = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
					    vec4 col = vec4(1.0);
					    col.rgb *= 1.0 - smoothstep(iRad, oRad, length(cent));
					    col *= tex;
					    col = mix(tex, col, opac);
					    COLOR = col;
					}"
			}
		}
	};

	public override void _Ready() {
		InitVignette();
	}

	public override void _Draw() {
		DrawBorder(this);
		ptsToDraw.ForEach(z => DrawCircle(z.pos, z.size, z.color));
	}

	private void InitVignette() {
		vignette.RectMinSize = GetViewportRect().Size;
		AddChild(vignette);
		if (Lib.Node.VignetteEnabled) {
			vignette.Show();
		} else {
			vignette.Hide();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventScreenDrag td) {
			if (td.Index.Equals(0)) {
				currentColor.ToHsv(out var hue, out var sat, out var val);
				currentColor = Color.FromHsv(hue += 0.01f, sat, val);
				ptsToDraw.Add((td.Position, 20, currentColor));
				Update();
			}
		}
	}

	public static void DrawBorder(CanvasItem canvas) {
		if (Lib.Node.BoderEnabled) {
			var vps = canvas.GetViewportRect().Size;
			int thickness = 4;
			var color = new Color(Lib.Node.BorderColorHtmlCode);
			canvas.DrawLine(new Vector2(0, 1), new Vector2(vps.x, 1), color, thickness);
			canvas.DrawLine(new Vector2(1, 0), new Vector2(1, vps.y), color, thickness);
			canvas.DrawLine(new Vector2(vps.x - 1, vps.y), new Vector2(vps.x - 1, 1), color, thickness);
			canvas.DrawLine(new Vector2(vps.x, vps.y - 1), new Vector2(1, vps.y - 1), color, thickness);
		}
	}
}
