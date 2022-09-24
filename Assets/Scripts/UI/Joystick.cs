using System;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    [SerializeField] private RectTransform slider;

    public bool pressed = false;
    private Vector2 sliderOrigin = new(0, 0);

    public Vector2 JoystickBounds => new Vector2(330, 330);

    public void Pressed()
    {
        pressed = true;
        MoveSlider();
    }

    private void MoveSlider()
    {
        var position = Input.touchCount > 0 ? Input.touches[0].position : (Vector2)Input.mousePosition;
        var r = 165;
        var dirVector = position - new Vector2(r, r);
        if (dirVector.sqrMagnitude > r * r)
        {
            position = dirVector.normalized * r + new Vector2(r, r);
        }
            
        slider.position = position;
        GameManager.Instance.OnMoveCommand(slider.anchoredPosition.normalized);
    }

    private void Update()
    {
        if (pressed)
        {
            MoveSlider();

            var touchUp = Input.touchCount > 0 ? Input.touches[0].phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);

            if (touchUp)
            {
                GameManager.Instance.OnMoveCommand(Vector2.zero);
                pressed = false;
                slider.anchoredPosition = sliderOrigin;
            }
        }
    }
}
