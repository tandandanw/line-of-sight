using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    Vector2 input;

    private void OnMove(InputValue inputValue)
    {
        input = inputValue.Get<Vector2>();
        transform.position += new Vector3(input.x, input.y, 0);
    }

}
