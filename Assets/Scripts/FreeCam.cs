using UnityEngine;
using System.Collections.Generic;

namespace PBDFluid
{

	public class FreeCam : MonoBehaviour 
	{

		public float m_speed = 50.0f;

		public float sensitivityX = 15;
		public float sensitivityY = 15;
		
		public float minimumX = -360;
		public float maximumX = 360;
		
		public float minimumY = -89F;
		public float maximumY = 89;
		
		private float rotationY = 0F;

		void Start () 
		{
            rotationY = transform.localEulerAngles.x;
			transform.localEulerAngles = new Vector3(rotationY, transform.localEulerAngles.y, 0);
		}

        void Update()
        {

            float speed = m_speed;

            if (Input.GetKey(KeyCode.Space)) speed *= 10.0f;

            Vector3 move = new Vector3(0, 0, 0);

            float deltaTime = Time.deltaTime;

            //move left
            if (Input.GetKey(KeyCode.A))
                move = new Vector3(-1, 0, 0) * deltaTime * speed;

            //move right
            if (Input.GetKey(KeyCode.D))
                move = new Vector3(1, 0, 0) * deltaTime * speed;

            //move forward
            if (Input.GetKey(KeyCode.W))
                move = new Vector3(0, 0, 1) * deltaTime * speed;

            //move back
            if (Input.GetKey(KeyCode.S))
                move = new Vector3(0, 0, -1) * deltaTime * speed;

            //move up
            if (Input.GetKey(KeyCode.Q))
                move = new Vector3(0, -1, 0) * deltaTime * speed;

            //move down
            if (Input.GetKey(KeyCode.E))
                move = new Vector3(0, 1, 0) * deltaTime * speed;
            if (Input.GetKey(KeyCode.Alpha1))
            { 
                transform.position = new Vector3(-5, 35, 88);
                transform.eulerAngles = new Vector3(25, 180, 0);
            }
            if (Input.GetKey(KeyCode.Alpha2))
            {
                transform.position = new Vector3(-26, 20, 42);
                transform.eulerAngles = new Vector3(0, 90, 0);
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                transform.position = new Vector3(-66, 150, 16);
                transform.eulerAngles = new Vector3(65, 90, 0);
            }

            transform.Translate(move);

			if(Input.GetMouseButton(1))
			{
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
            }

		
		}
	}

}
