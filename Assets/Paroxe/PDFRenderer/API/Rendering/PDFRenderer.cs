using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Paroxe.PdfRenderer.Internal;
using UnityEngine;
using Paroxe.PdfRenderer.WebGL;
using System.Collections; // For WebGL

namespace Paroxe.PdfRenderer
{
	/// <summary>
	/// This class allow the application to render pages into textures.
	/// </summary>
	public sealed class PDFRenderer : IDisposable
	{
		private PDFBitmap m_Bitmap;
		private byte[] m_IntermediateBuffer;

		~PDFRenderer()
		{
			Close();
		}

		public void Dispose()
		{
			Close();

			GC.SuppressFinalize(this);
		}

		private void Close()
		{
			if (m_Bitmap == null)
				return;

			m_Bitmap.Dispose();
			m_Bitmap = null;
		}

		public static PDFJS_Promise<Texture2D> RenderPageToExistingTextureAsync(PDFPage page, Texture2D tex)
		{
			PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();
			using (PDFRenderer renderer = new PDFRenderer())
			{
				renderPromise.HasFinished = true;
				renderPromise.HasSucceeded = true;
				renderPromise.HasReceivedJSResponse = true;
				renderer.RenderPageToExistingTexture(page, tex);
				renderPromise.Result = tex;
			}
			return renderPromise;
		}

		public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, int width, int height)
		{
			return RenderPageToTextureAsync(page, new Vector2(width, height));
		}

		public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, Vector2 size)
		{
			PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();
			using (PDFRenderer renderer = new PDFRenderer())
			{
				renderPromise.HasFinished = true;
				renderPromise.HasSucceeded = true;
				renderPromise.HasReceivedJSResponse = true;
				renderPromise.Result = renderer.RenderPageToTexture(page, (int)size.x, (int)size.y);
			}
			return renderPromise;
		}

		public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, float scale = 1.0f)
		{
			PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();

			using (PDFRenderer renderer = new PDFRenderer())
			{
				renderPromise.HasFinished = true;
				renderPromise.HasSucceeded = true;
				renderPromise.HasReceivedJSResponse = true;
				Vector2 size = page.GetPageSize(scale);
				renderPromise.Result = renderer.RenderPageToTexture(page, (int)size.x, (int)size.y);
			}

			return renderPromise;
		}

        /// <summary>
        /// Render page into a new byte array.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
		public byte[] RenderPageToByteArray(PDFPage page)
		{
			return RenderPageToByteArray(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y, null,
				RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new byte array.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public byte[] RenderPageToByteArray(PDFPage page, int width, int height)
		{
			return RenderPageToByteArray(page, width, height, null, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new byte array.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="rectsProvider"></param>
		/// <returns></returns>
		public byte[] RenderPageToByteArray(PDFPage page, int width, int height,
			IPDFColoredRectListProvider rectsProvider)
		{
			return RenderPageToByteArray(page, width, height, rectsProvider, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new byte array.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="rectsProvider"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public byte[] RenderPageToByteArray(PDFPage page, int width, int height,
			IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
		{
			if (settings == null)
				settings = RenderSettings.defaultRenderSettings;

			if (m_Bitmap == null || m_Bitmap.UseAlphaChannel != settings.transparentBackground || !m_Bitmap.HasSameSize(width, height))
			{
				if (m_Bitmap != null)
					m_Bitmap.Dispose();

				m_Bitmap = new PDFBitmap(width, height, settings.transparentBackground);
			}

			m_Bitmap.FillRect(0, 0, width, height, settings.transparentBackground ? 0x00000000 : int.MaxValue);

			int flags = settings == null
				? RenderSettings.defaultRenderSettings.ComputeRenderingFlags()
				: settings.ComputeRenderingFlags();

			float scale = width / page.GetPageSize(1.0f).x;

			PDFRect clipping = new PDFRect(width, height);
			PDFMatrix matrix = PDFMatrix.Identity;
			matrix.Scale(scale, -scale);
			matrix.Translate(0.0f, height);

			NativeMethods.FPDF_RenderPageBitmapWithMatrix(m_Bitmap.NativePointer, page.NativePointer, ref matrix, ref clipping, flags);

			IntPtr bufferPtr = m_Bitmap.GetBuffer();

			if (bufferPtr == IntPtr.Zero)
				return null;

			int length = width * height * 4;

			if (m_IntermediateBuffer == null || m_IntermediateBuffer.Length < length)
				m_IntermediateBuffer = new byte[width * height * 4];

			Marshal.Copy(bufferPtr, m_IntermediateBuffer, 0, width * height * 4);

			IList<PDFColoredRect> coloredRects = rectsProvider != null
				? rectsProvider.GetBackgroundColoredRectList(page)
				: null;

			if (coloredRects != null && coloredRects.Count > 0)
			{
				foreach (PDFColoredRect coloredRect in coloredRects)
				{
					int r = (int)(coloredRect.Color.r * 255) & 0xFF;
					int g = (int)(coloredRect.Color.g * 255) & 0xFF;
					int b = (int)(coloredRect.Color.b * 255) & 0xFF;
					int a = (int)(coloredRect.Color.a * 255) & 0xFF;

					float alpha = (a / (float)255);
					float reverseAlpha = 1.0f - alpha;

					Rect deviceRect = page.ConvertPageRectToDeviceRect(coloredRect.PageRect, new Vector2(width, height));

					if (deviceRect.x >= 0.0f
					    && deviceRect.y >= 0.0f
					    && deviceRect.x + deviceRect.width <= width
					    && deviceRect.y + deviceRect.height <= height)
					{
						for (int y = 0; y < (int)deviceRect.height; ++y)
						{
							for (int x = 0; x < (int)deviceRect.width; ++x)
							{
								int s = (((height - (int)deviceRect.y) - y) * width + (int)deviceRect.x + x) * 4;

								byte sr = m_IntermediateBuffer[s];
								byte sg = m_IntermediateBuffer[s + 1];
								byte sb = m_IntermediateBuffer[s + 2];

								m_IntermediateBuffer[s] = (byte)Mathf.Clamp(alpha * r + (reverseAlpha * sr), 0, 255);
								m_IntermediateBuffer[s + 1] = (byte)Mathf.Clamp(alpha * g + (reverseAlpha * sg), 0, 255);
								m_IntermediateBuffer[s + 2] = (byte)Mathf.Clamp(alpha * b + (reverseAlpha * sb), 0, 255);
								m_IntermediateBuffer[s + 3] = 0xFF;
							}
						}
					}
				}
			}
			return m_IntermediateBuffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PDFMatrix
		{
			public static PDFMatrix Identity
			{
				get { return new PDFMatrix { a = 1, b = 0, c = 0, d = 1, e = 0, f = 0 }; }
			}

			public float a;
			public float b;
			public float c;
			public float d;
			public float e;
			public float f;

			public PDFMatrix(float[] n)
			{
				if (n == null)
					throw new ArgumentNullException("n");
				if (n.Length != 6)
					throw new ArgumentException("n must have 6 elements", "n");

				a = n[0];
				b = n[1];
				c = n[2];
				d = n[3];
				e = n[4];
				f = n[5];
			}

			public PDFMatrix(float a, float b, float c, float d, float e, float f)
			{
				this.a = a;
				this.b = b;
				this.c = c;
				this.d = d;
				this.e = e;
				this.f = f;
			}

			public void SetIdentity()
			{
				a = 1;
				b = 0;
				c = 0;
				d = 1;
				e = 0;
				f = 0;
			}

			public void Scale(float sx, float sy, bool prepended = false)
			{
				a *= sx;
				d *= sy;
				if (prepended)
				{
					b *= sx;
					c *= sy;
					return;
				}

				b *= sy;
				c *= sx;
				e *= sx;
				f *= sy;
			}

			public void Translate(float x, float y, bool prepended = false)
			{
				if (prepended)
				{
					e += x * a + y * c;
					f += y * d + x * b;
					return;
				}
				e += x;
				f += y;
			}

			public void Rotate(float radian, bool prepended = false)
			{
				float cosValue = Mathf.Cos(radian);
				float sinValue = Mathf.Sin(radian);
				ConcatInternal(new PDFMatrix(cosValue, sinValue, -sinValue, cosValue, 0, 0), prepended);
			}

			public void RotateAt(float fRadian, float dx, float dy, bool prepended = false)
			{
				Translate(dx, dy, prepended);
				Rotate(fRadian, prepended);
				Translate(-dx, -dy, prepended);
			}

			private void ConcatInternal(PDFMatrix other, bool prepend)
			{
				PDFMatrix left;
				PDFMatrix right;

				if (prepend)
				{
					left = other;
					right = this;
				}
				else
				{
					left = this;
					right = other;
				}

				a = left.a * right.a + left.b * right.c;
				b = left.a * right.b + left.b * right.d;
				c = left.c * right.a + left.d * right.c;
				d = left.c * right.b + left.d * right.d;
				e = left.e * right.a + left.f * right.c + right.e;
				f = left.e * right.b + left.f * right.d + right.f;
			}

			public float this[int index]
			{
				get
				{
					float num;
					switch (index)
					{
						case 0:
							{
								num = a;
								break;
							}
						case 1:
							{
								num = b;
								break;
							}
						case 2:
							{
								num = c;
								break;
							}
						case 3:
							{
								num = d;
								break;
							}
						case 4:
							{
								num = e;
								break;
							}
						case 5:
							{
								num = f;
								break;
							}
						default:
							{
								throw new IndexOutOfRangeException(string.Format("Invalid PDFMatrix index addressed: {0}!", new object[] { index }));
							}
					}
					return num;
				}
				set
				{
					switch (index)
					{
						case 0:
							{
								a = value;
								break;
							}
						case 1:
							{
								b = value;
								break;
							}
						case 2:
							{
								c = value;
								break;
							}
						case 3:
							{
								d = value;
								break;
							}
						case 4:
							{
								e = value;
								break;
							}
						case 5:
							{
								f = value;
								break;
							}
						default:
							{
								throw new IndexOutOfRangeException(string.Format("Invalid PDFMatrix index addressed: {0}!", new object[] { index }));
							}
					}
				}
			}

			public override bool Equals(object other)
			{
				return (other is PDFMatrix ? this == (PDFMatrix)other : false);
			}

			public static bool operator ==(PDFMatrix lhs, PDFMatrix rhs)
			{
				return (lhs.a != rhs.a || lhs.b != rhs.b || lhs.c != rhs.c || lhs.d != rhs.d || lhs.e != rhs.e ? false : lhs.f == rhs.f);
			}

			public static bool operator !=(PDFMatrix lhs, PDFMatrix rhs)
			{
				return !(lhs == rhs);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = this.a.GetHashCode();
					hashCode = hashCode * 23 + b.GetHashCode();
					hashCode = hashCode * 23 + c.GetHashCode();
					hashCode = hashCode * 23 + d.GetHashCode();
					hashCode = hashCode * 23 + e.GetHashCode();
					hashCode = hashCode * 23 + f.GetHashCode();
					return hashCode;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PDFRect
		{
			public float left;
			public float top;
			public float right;
			public float bottom;

			public PDFRect(float left, float top, float right, float bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public PDFRect(float width, float height)
			{
				left = 0;
				top = 0;
				right = width;
				bottom = height;
			}

			public override bool Equals(object other)
			{
				return (other is PDFRect ? this == (PDFRect)other : false);
			}

			public static bool operator ==(PDFRect lhs, PDFRect rhs)
			{
				return (lhs.left != rhs.left || lhs.top != rhs.top || lhs.right != rhs.right ? false : lhs.bottom == rhs.bottom);
			}

			public static bool operator !=(PDFRect lhs, PDFRect rhs)
			{
				return !(lhs == rhs);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = this.left.GetHashCode();
					hashCode = hashCode * 23 + top.GetHashCode();
					hashCode = hashCode * 23 + right.GetHashCode();
					hashCode = hashCode * 23 + bottom.GetHashCode();
					return hashCode;
				}
			}
		}

		/// <summary>
		/// Render page into a new Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>]
		public Texture2D RenderPageToTexture(PDFPage page)
		{
			return RenderPageToTexture(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y, null,
				RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public Texture2D RenderPageToTexture(PDFPage page, int width, int height)
		{
			return RenderPageToTexture(page, width, height, null, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="rectsProvider"></param>
		/// <returns></returns>
		public Texture2D RenderPageToTexture(PDFPage page, int width, int height,
			IPDFColoredRectListProvider rectsProvider)
		{
			return RenderPageToTexture(page, width, height, rectsProvider, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into a new Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="rectsProvider"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public Texture2D RenderPageToTexture(PDFPage page, int width, int height,
			IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
		{
			Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

			RenderPageToExistingTexture(page, newTex, rectsProvider, settings);

			return newTex;
		}

		/// <summary>
		/// Render page into an existing Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="texture"></param>
		public void RenderPageToExistingTexture(PDFPage page, Texture2D texture)
		{
			RenderPageToExistingTexture(page, texture, null, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into an existing Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="texture"></param>
		/// <param name="rectsProvider"></param>
		public void RenderPageToExistingTexture(PDFPage page, Texture2D texture,
			IPDFColoredRectListProvider rectsProvider)
		{
			RenderPageToExistingTexture(page, texture, rectsProvider, RenderSettings.defaultRenderSettings);
		}

		/// <summary>
		/// Render page into an existing Texture2D.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="texture"></param>
		/// <param name="rectsProvider"></param>
		/// <param name="settings"></param>
		public void RenderPageToExistingTexture(PDFPage page, Texture2D texture,
			IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
		{
			byte[] byteArray = RenderPageToByteArray(page, texture.width, texture.height, rectsProvider, settings);

			if (byteArray != null)
			{
				texture.wrapMode = TextureWrapMode.Clamp;

				if ((texture.format != TextureFormat.RGBA32
					 && texture.format != TextureFormat.ARGB32
					 && texture.format != TextureFormat.BGRA32
					 && texture.format != (TextureFormat)37) || texture.mipmapCount > 1)
				{
					Color32[] pixels = new Color32[texture.width * texture.height];

					for (int i = 0; i < pixels.Length; ++i)
						pixels[i] = new Color32(
							byteArray[i * 4],
							byteArray[i * 4 + 1],
							byteArray[i * 4 + 2],
							byteArray[i * 4 + 3]);

					texture.SetPixels32(pixels);
					texture.Apply();
				}
				else
				{
					texture.LoadRawTextureData(byteArray);
					texture.Apply();
				}
			}
		}

		/// <summary>
		/// Allows the application to specify render settings.
		/// </summary>
		[Serializable]
		public class RenderSettings
		{
			public bool disableSmoothPath = false;
			public bool disableSmoothText = false;
			public bool disableSmoothImage = false;
			public bool grayscale = false;
			public bool optimizeTextForLCDDisplay = false;
			public bool renderAnnotations = false;
			public bool renderForPrinting = false;
			public bool transparentBackground = false;

			public static RenderSettings defaultRenderSettings
			{
				get { return new RenderSettings(); }
			}

			public int ComputeRenderingFlags()
			{
				int flags = 0x10;

				if (renderAnnotations)
					flags |= 0x01;
				if (optimizeTextForLCDDisplay)
					flags |= 0x02;
				if (grayscale)
					flags |= 0x08;
				if (renderForPrinting)
					flags |= 0x800;
				if (disableSmoothText)
					flags |= 0x1000;
				if (disableSmoothImage)
					flags |= 0x2000;
				if (disableSmoothPath)
					flags |= 0x4000;

				return flags;
			}
		}
	}
}