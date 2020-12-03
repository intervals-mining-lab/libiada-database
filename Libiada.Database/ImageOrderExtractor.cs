using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Libiada.Database
{
    public enum ImageOrderExtractor : byte
    {
        /// <summary>
        /// Reads image line by line from top to bottom and from left to right.
        /// </summary>
        [Display(Name = "Read image left to right top to bottom")]
        [Description("Reads image line by line from top to bottom and from left to right")]
        LineLeftToRightTopToBottom = 1,

        [Display(Name = "Read image left to right bottom to top")]
        [Description("Reads image line by line from bottom to top and from left to right")]
        LineLeftToRightBottomToTop = 2,

        [Display(Name = "Read image right to left top to bottom")]
        [Description("Reads image line by line from top to bottom and from right to left")]
        LineRightToLeftTopToBottom = 3,

        [Display(Name = "Read image right to left bottom to top")]
        [Description("Reads the image line by line from bottom to top and from right to left")]
        LineRightToLeftBottomToTop = 4,

        [Display(Name = "Read top to bottom image of the zigzag left corner")]
        [Description("Reads a zigzag image of the left corner from top to bottom")]
        LineZigzagLeftCornerTopToBottom = 5,

        [Display(Name = "Read bottom to top image of the zigzag left corner")]
        [Description("Reads a zigzag image of the left corner from bottom to top")]
        LineZigzagLeftCornerBottomToTop = 6,

        [Display(Name = "Read top to bottom image of the zigzag right corner")]
        [Description("Reads a zigzag image of the right corner from top to bottom")]
        LineZigzagRightCornerTopToBottom = 7,

        [Display(Name = "Read bottom to top image of the zigzag right corner")]
        [Description("Reads a zigzag image of the right corner from bottom to top")]
        LineZigzagRightCornerBottomToTop = 8,

        [Display(Name = "Read from top to bottom column from left to right")]
        [Description("Reads an image from top to bottom, column from left to right")]
        ColumnTopToBottomLeftToRight = 9,

        [Display(Name = "Read from top to bottom column from right to left")]
        [Description("Reads an image from top to bottom, column from right to left")]
        ColumnTopToBottomRightToLeft = 10,

        [Display(Name = "Read from bottom to top column from left to right")]
        [Description("Reads an image from bottom to top, column from left to right")]
        ColumnBottomToTopLeftToRight = 11,

        [Display(Name = "Read from bottom to top column from right to left")]
        [Description("Reads an image from bottom to top, column from right to left")]
        ColumnBottomToTopRightToLeft = 12,

        [Display(Name = "Read spiral from the center clockwise")]
        [Description("Reads a spiral image from the center clockwise")]
        SpiralFromCenterClockwise = 13,
        
        [Display(Name = "Read spiral from the center counter clockwise")]
        [Description("Reads a spiral image from the center counter clockwise")]
        SpiralFromCenterCounterClockwise = 14,

        [Display(Name = "Read spiral from the upper left corner clockwise")]
        [Description("Reads a spiral image from the upper left corner clockwise")]
        SpiralFromLeftTopCornerClockwise = 15,

        [Display(Name = "Read spiral from the upper right corner clockwise")]
        [Description("Reads a spiral image from the upper right corner clockwise")]
        SpiralFromRightTopCornerClockwise = 16,

        [Display(Name = "Read spiral from the lower left corner clockwise")]
        [Description("Reads a spiral image from the lower left corner clockwise")]
        SpiralFromLeftBottomCornerClockwise = 17,

        [Display(Name = "Read spiral from the lower right corner clockwise")]
        [Description("Reads a spiral image from the lower right corner clockwise")]
        SpiralFromRightBottomCornerClockwise = 18,

        [Display(Name = "Read spiral from the upper left corner counter clockwise")]
        [Description("Reads a spiral image from the upper left corner counter clockwise")]
        SpiralFromLeftTopCornerCounterClockwise = 19,

        [Display(Name = "Read spiral from the upper right corner counter clockwise")]
        [Description("Reads a spiral image from the upper right corner counter clockwise")]
        SpiralFromRightTopCornerCounterClockwise = 20,

        [Display(Name = "Read spiral from the lower left corner counterclockwise")]
        [Description("Reads a spiral image from the lower left corner counterclockwise")]
        SpiralFromLeftBottomCornerCounterClockwise = 21,

        [Display(Name = "Read spiral from the lower right corner counterclockwise")]
        [Description("Reads a spiral image from the lower right corner counterclockwise")]
        SpiralFromRightBottomCornerCounterClockwise = 22,
    }
}