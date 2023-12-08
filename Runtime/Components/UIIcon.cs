using UnityEngine;

namespace PluginSet.UGUI
{
    public class UIIcon: MonoBehaviour, IIconProtocol
    {
        [SerializeField]
        private UIPackageSpriteIcon localIcon;
        
        [SerializeField]
        private UILoader remoteIcon;
        
        private string _icon;

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
        
        public void SetIcon(string value)
        {
            _icon = value;
            if (value.StartsWith(UIPackage.URL_PREFIX))
            {
                localIcon.icon = value;
                ShowLocalIcon();
            }
            else
            {
                remoteIcon.icon = value;
                ShowRemoteIcon();
            }
        }
        
        private void ShowLocalIcon()
        {
            localIcon.gameObject.SetActive(true);
            remoteIcon.gameObject.SetActive(false);
        }
        
        private void ShowRemoteIcon()
        {
            localIcon.gameObject.SetActive(false);
            remoteIcon.gameObject.SetActive(true);
        }

        public void SetTexture(Texture2D texture2D)
        {
            remoteIcon.SetTexture(texture2D);
            ShowRemoteIcon();
        }

        public void SetSprite(Sprite sprite)
        {
            localIcon.SetSprite(sprite);
            ShowLocalIcon();
        }
    }
}