using UnityEngine;

namespace DeepSpace.LaserTracking
{
    [System.Serializable]
    public class TrackingSettings
    {
        [SerializeField, Tooltip("Value in pixel.")]
        protected float screenWidthPixel = 19.2f;
        [SerializeField, Tooltip("Value in pixel.")]
        protected float screenHeightPixel = 10.8f;
        [SerializeField, Tooltip("Value in centimeter.")]
        protected float stageWidth = 1600f;
        [SerializeField, Tooltip("Value in centimeter.")]
        protected float stageHeight = 900f;

        public float ScreenWidthPixel
        {
            get { return this.screenWidthPixel; }
            set { this.screenWidthPixel = value; }
        }

        public float ScreenHeightPixel
        {
            get { return this.screenHeightPixel; }
            set { this.screenHeightPixel = value; }
        }

        public float StageWidth
        {
            get { return this.stageWidth; }
            set { this.stageWidth = value; }
        }

        public float StageHeight
        {
            get { return this.stageHeight; }
            set { this.stageHeight = value; }
        }

        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2(/*(int)Mathf.Round*/(x * screenWidthPixel), screenHeightPixel - /*(int) Mathf.Round*/(y * screenHeightPixel));
        }
    }
}