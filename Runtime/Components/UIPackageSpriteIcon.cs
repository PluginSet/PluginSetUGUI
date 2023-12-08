using System;
using UnityEngine;
using UnityEngine.UI;

namespace PluginSet.UGUI
{
    [RequireComponent(typeof(Image))]
    public class UIPackageSpriteIcon: MonoBehaviour, IIconProtocol
    {
        private string _icon;

        [SerializeField]
        private Image image;
        
        public string icon
        {
            get => _icon;

            set
            {
                if (string.Equals(_icon, value))
                    return;
                
                SetIcon(value);
            }
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void SetIcon(string value)
        {
            _icon = value;
            LoadSprite(value);
        }

        public void SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
        }

        private void LoadSprite(string url)
        {
            SetSprite(UIPackage.GetPackageItem(url).asset as Sprite);
        }
        
#if UNITY_EDITOR
        private void InitImage()
        {
            image = GetComponent<Image>();
        }
        
        private void OnEnable()
        {
            InitImage();
        }

        private void OnValidate()
        {
            InitImage();
        }
#endif
    }
}