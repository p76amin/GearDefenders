using TMPro;
using UnityEngine;

namespace GearDefenders
{
    public class OverlayPopupUI : MonoBehaviour
    {
        TextMeshProUGUI _title;
        TextMeshProUGUI _body;

        public void Bind(TextMeshProUGUI title, TextMeshProUGUI body)
        {
            _title = title;
            _body = body;
            Hide();
        }

        public void Show(string title, string body)
        {
            _title.text = title;
            _body.text = body;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
