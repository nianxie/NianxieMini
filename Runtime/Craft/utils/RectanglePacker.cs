using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Nianxie.Craft
{
	public static class ListExtension
	{
		public static T Pop<T>(this List<T> list)
		{
			int index = list.Count - 1;
			T r = list[index];
			list.RemoveAt(index);
			return r;
		}
	}

	/**
     * Class used to store rectangles values inside rectangle packer
     */
	public class IntRectangle {
		public int x;
		public int y;
		public int width;
		public int height;
		[JsonIgnore]
		public int right => x+width;
		[JsonIgnore]
		public int bottom => y+height;
		public IntRectangle(int x = 0, int y = 0, int width = 0, int height = 0)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public Rect ToUnityRect()
		{
			return new Rect(x, y, width, height);
		}
	}

	// https://github.com/villekoskelaorg/RectanglePacking
    /**
     * Class used to pack rectangles within container rectangle with close to optimal solution.
     */
    public class RectanglePacker {
	    /// <summary>
	    /// 将固定width、height的rect打包，得到结果的IntRectangle，并返回finalSize
	    /// </summary>
	    /// <param name="sortedRect"></param>
	    /// <returns></returns>
	    public static Vector2Int PackRectsInplace(IntRectangle[] sortedRectSize)
	    {
		    return PackRectsInplace_TestFromBaseSize(sortedRectSize, 512);
	    }

	    // 从一个给定的baseSize对应的尺寸baseSize*baseSize不断翻倍尝试打包矩形
	    private static Vector2Int PackRectsInplace_TestFromBaseSize(IntRectangle[] sortedRect, int baseSize)
	    {
		    var allProduct = sortedRect.Select(r => r.width * r.height).Sum();
		    while (baseSize * baseSize < allProduct)
		    {
			    baseSize *= 2;
		    }
		    // 开始尝试打包矩形
		    var curWidth = baseSize;
		    var curHeight = baseSize;
		    while (true)
		    {
				var packer = new RectanglePacker(curWidth, curHeight);
				bool fail = false;
				for (int i = 0; i < sortedRect.Length; i++)
				{
					var curRect = sortedRect[i];
					var addRect = packer.AddRectangle(curRect.width, curRect.height);
					if (addRect != null)
					{
						curRect.x = addRect.x;
						curRect.y = addRect.y;
					}
					else
					{
						fail = true;
						break;
					}
				}

				// 如果失败，就翻倍长或宽
				if (fail)
				{
					if (curWidth >= curHeight)
					{
						curHeight *= 2;
					}
					else
					{
						curWidth *= 2;
					}
					continue;
				}
				return new Vector2Int(packer.packedWidth, packer.packedHeight);
		    }
	    }

	    //static public readonly string VERSION = "1.3.0";
	    
		private int mWidth = 0;
		private int mHeight = 0;
		private int mPadding = 8;

		private int mPackedWidth = 0;
		private int mPackedHeight = 0;

		private List<Vector2Int> mInsertList = new List<Vector2Int>();

		private List<IntRectangle> mInsertedRectangles = new List<IntRectangle>();
		private List<IntRectangle> mFreeAreas = new List<IntRectangle>();
		private List<IntRectangle> mNewFreeAreas = new List<IntRectangle>();

		private IntRectangle mOutsideRectangle;

		private List<Vector2Int> mVector2IntStack = new List<Vector2Int>();
		private List<IntRectangle> mRectangleStack = new List<IntRectangle>();

		public int rectangleCount { get { return mInsertedRectangles.Count; } }

		public int packedWidth { get { return mPackedWidth; } }
		public int packedHeight { get { return mPackedHeight; } }

		public int padding { get { return mPadding; } }

		public RectanglePacker(int width, int height, int padding = 0) {
			mOutsideRectangle = new IntRectangle(width + 1, height + 1, 0, 0);
			Reset(width, height, padding);
		}

		public void Reset(int width, int height, int padding = 0) {
			while (mInsertedRectangles.Count > 0)
				FreeRectangle(mInsertedRectangles.Pop());

			while (mFreeAreas.Count > 0)
				FreeRectangle(mFreeAreas.Pop());

			mWidth = width;
			mHeight = height;

			mPackedWidth = 0;
			mPackedHeight = 0;

			mFreeAreas.Add(AllocateRectangle(0, 0, mWidth, mHeight));

			while (mInsertList.Count > 0)
				FreeSize(mInsertList.Pop());

			mPadding = padding;
		}

		public IntRectangle GetRectangle(int index, IntRectangle rectangle) {
			IntRectangle inserted = mInsertedRectangles[index];

			rectangle.x = inserted.x;
			rectangle.y = inserted.y;
			rectangle.width = inserted.width;
			rectangle.height = inserted.height;

			return rectangle;
		}

		public IntRectangle AddRectangle(int width, int height) {
			return PackRectangle(AllocateSize(width, height));
		}

		public IntRectangle PackRectangle(Vector2Int sortableSize) {
			int width = sortableSize.x;
            int height = sortableSize.y;

			int index = GetFreeAreaIndex(width, height);
			IntRectangle target = null;
			if (index >= 0) {

				IntRectangle freeArea = mFreeAreas[index];
				target = AllocateRectangle(freeArea.x, freeArea.y, width, height);

				// Generate the new free areas, these are parts of the old ones intersected or touched by the target
				GenerateNewFreeAreas(target, mFreeAreas, mNewFreeAreas);

				while (mNewFreeAreas.Count > 0)
					mFreeAreas.Add(mNewFreeAreas.Pop());

				mInsertedRectangles.Add(target);

				if (target.right > mPackedWidth)
					mPackedWidth = target.right;
				
				if (target.bottom > mPackedHeight)
					mPackedHeight = target.bottom;
			}

			FreeSize(sortableSize);

			return target;
		}

		private void FilterSelfSubAreas(List<IntRectangle> areas) {
			for (int i = areas.Count - 1; i >= 0; i--) {
				IntRectangle filtered = areas[i];

				for (int j = areas.Count - 1; j >= 0; j--) {
					if (i != j) {

						IntRectangle area = areas[j];
						if (filtered.x >= area.x && filtered.y >= area.y && filtered.right <= area.right && filtered.bottom <= area.bottom) {

							FreeRectangle(filtered);
							IntRectangle topOfStack = areas.Pop();
							if (i < areas.Count) {

								// Move the one on the top to the freed position
								areas[i] = topOfStack;
							}
							break;
						}
					}
				}
			}
		}

		private void GenerateNewFreeAreas(IntRectangle target, List<IntRectangle> areas, List<IntRectangle> results) {
			// Increase dimensions by one to get the areas on right / bottom this rectangle touches
			// Also add the padding here
			float x = target.x;
			float y = target.y;
			float right = target.right + 1 + mPadding;
			float bottom = target.bottom + 1 + mPadding;

			IntRectangle targetWithPadding = null;
			if (mPadding == 0)
				targetWithPadding = target;

			for (int i = areas.Count - 1; i >= 0; i--) {
				IntRectangle area = areas[i];
				if (!(x >= area.right || right <= area.x || y >= area.bottom || bottom <= area.y)) {

					if (targetWithPadding == null)
						targetWithPadding = AllocateRectangle(target.x, target.y, target.width + mPadding, target.height + mPadding);

					GenerateDividedAreas(targetWithPadding, area, results);
					IntRectangle topOfStack = areas.Pop();
					if (i < areas.Count) {

						// Move the one on the top to the freed position
						areas[i] = topOfStack;
					}
				}
			}

			if (targetWithPadding != null && targetWithPadding != target)
				FreeRectangle(targetWithPadding);

			FilterSelfSubAreas(results);
		}

		private void GenerateDividedAreas(IntRectangle divider, IntRectangle area, List<IntRectangle> results) {
			int count = 0;

            int rightDelta = area.right - divider.right;
			if (rightDelta > 0) {
				results.Add(AllocateRectangle(divider.right, area.y, rightDelta, area.height));
				count++;
			}

            int leftDelta = divider.x - area.x;
			if (leftDelta > 0) {
				results.Add(AllocateRectangle(area.x, area.y, leftDelta, area.height));
				count++;
			}

            int bottomDelta = area.bottom - divider.bottom;
			if (bottomDelta > 0) {
				results.Add(AllocateRectangle(area.x, divider.bottom, area.width, bottomDelta));
				count++;
			}

            int topDelta = divider.y - area.y;
			if (topDelta > 0) {
				results.Add(AllocateRectangle(area.x, area.y, area.width, topDelta));
				count++;
			}

			if (count == 0 && (divider.width < area.width || divider.height < area.height)) {
				// Only touching the area, store the area itself
				results.Add(area);
			} else
				FreeRectangle(area);
		}

		private int GetFreeAreaIndex(int width, int height) {
			IntRectangle best = mOutsideRectangle;
			int index = -1;

			float paddedWidth = width + mPadding;
			float paddedHeight = height + mPadding;

			int count = mFreeAreas.Count;
			for (int i = count - 1; i >= 0; i--) {
				IntRectangle free = mFreeAreas[i];
				if (free.x < mPackedWidth || free.y < mPackedHeight) {

					// Within the packed area, padding required
					if (free.x < best.x && paddedWidth <= free.width && paddedHeight <= free.height) {

						index = i;
						if ((paddedWidth == free.width && free.width <= free.height && free.right < mWidth) || (paddedHeight == free.height && free.height <= free.width))
							break;
						
						best = free;
					}
				} else {
					// Outside the current packed area, no padding required
					if (free.x < best.x && width <= free.width && height <= free.height) {

						index = i;
						if ((width == free.width && free.width <= free.height && free.right < mWidth) || (height == free.height && free.height <= free.width))
							break;

						best = free;
					}
				}
			}

			return index;
		}

		private IntRectangle AllocateRectangle(int x, int y, int width, int height) {
			if (mRectangleStack.Count > 0) {
				IntRectangle rectangle = mRectangleStack.Pop();
				rectangle.x = x;
				rectangle.y = y;
				rectangle.width = width;
				rectangle.height = height;

				return rectangle;
			}
			return new IntRectangle(x, y, width, height);
		}

		private void FreeRectangle(IntRectangle rectangle) {
			mRectangleStack.Add(rectangle);
		}

		private Vector2Int AllocateSize(int width, int height) {
			if (mVector2IntStack.Count > 0) {
				Vector2Int size = mVector2IntStack.Pop();
				size.x = width;
				size.y = height;

				return size;
			}

			return new Vector2Int(width, height);
		}

		private void FreeSize(Vector2Int size) {
			mVector2IntStack.Add(size);
		}
    }
}