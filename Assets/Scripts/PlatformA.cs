using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class PlatformA : MonoBehaviour
{
    [SerializeField] private MovingPlatform movingPlatform;

    private Rigidbody2D _rb;

    private float _timeFlipped;
    private bool direction;
    private void Awake()
    {   
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        _rb.velocity = new Vector2(movingPlatform.speed, 0);
        _timeFlipped += Time.deltaTime;

        // Changes direction after a certain interval
        if (_timeFlipped > movingPlatform.invertTime)
        {
            // Inverts direction
            direction = !direction;
            _timeFlipped = 0;
        }

        // Applying velocity changes to platform 
        if (direction)
        {
            _rb.velocity = new Vector2(-movingPlatform.speed, 0);
        }
        else
        {
            _rb.velocity = new Vector2(movingPlatform.speed, 0);
        }
    }
}
