﻿using UnityEngine;

// Este es el script que hace girar el arma dependiendo
// donde se encuentra el mouse
public class FlipWithMouse : MonoBehaviour
{
    bool flipped; // Si ya hay que invertir el arma en el eje X
    Vector3 mousePos; // Posicion del mouse

    public float topMin = 45f;
    public float topMax = 135f;
    public float newPosX = 0.018f;

    bool atBackground = false;

    // Use this for initialization
    void Start()
    {
        mousePos = new Vector3();
        flipped = false;
    }

    // Update is called once per frame
    void Update()
    {
        float angle = getAngle();

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        bool isUp = angle > topMin && angle < topMax;
        bool isLeft = Mathf.Abs(angle) > topMax;
        bool isRight = Mathf.Abs(angle) < topMin;

        // El arma se invierte si nos pasamos de cierto angulo
        if (isLeft && !flipped)
        {
            flip();
        }
        else if (isRight && flipped)
        {
            flip();
        }

        if (!atBackground && isUp)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 1);
            atBackground = true;
        } else if (atBackground && !isUp)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
            atBackground = false;
        }
    }

    // Esta funcion invierte el arma
    void flip()
    {
        Vector3 position = transform.localPosition;

        if (!flipped)
        {
            position.x = newPosX * -1;
        }
        else
        {
            position.x = newPosX;
        }
        transform.localPosition = position;

        Vector3 scale = transform.localScale;
        scale.y *= -1;
        transform.localScale = scale;

        flipped = !flipped;
    }

    // Esta funcion devuelve el angulo que hay entre el arma y la posicion
    // del mouse
    float getAngle()
    {
        mousePos = Input.mousePosition;
        Vector2 distance = mousePos - Camera.main.WorldToScreenPoint(transform.position);
        return Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
    }

    // Devuelve si estamos apuntando a la derecha o a la izquierda del personaje
    bool isFacingRight(float angle)
    {
        if ((angle > 90 && angle < 180) || (angle < -90 && angle > -180))
        {
            return false;
        }

        return true;
    }
}
