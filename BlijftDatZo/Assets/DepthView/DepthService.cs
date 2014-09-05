﻿using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthService : MonoBehaviour
{
	private KinectSensor sensor;

	private DepthFrameReader  depthReader;

	private bool isDirty;

	private Texture2D texture;

	private ushort[] depth;

	/// <summary>
	/// Raw data to load in the texture
	/// </summary>
	private byte[] _RawData;

	void Start ()
	{
		this.sensor = KinectSensor.GetDefault();
		
		if (this.sensor != null) 
		{
			this.depthReader = sensor.DepthFrameSource.OpenReader();

			if (!this.sensor.IsOpen)
			{
				this.sensor.Open();
			}
		}
		//gameObject.renderer.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
	}
	
	// Update is called once per frame
	void Update ()
    {
        using (var depthFrame = depthReader.AcquireLatestFrame())
        {
            if (depthFrame != null)
            {
                var frameDesc = depthFrame.FrameDescription;
                if (this.texture == null)
                {
                    this.texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
                    if (this.depth == null || depth.Length != frameDesc.LengthInPixels)
                    {
                        this.depth = new ushort[frameDesc.LengthInPixels];
                        this._RawData = new byte[frameDesc.LengthInPixels * 4];
                    }
                }

                depthFrame.CopyFrameDataToArray(depth);
            }
            else
            {
                Debug.Log("depthFrame is null");
            }
        }

        int min = 500;
        int max = 4500;
        if (this.depth != null)
        {
            Debug.Log("loading texture");
            int index = 0;
            foreach (var ir in depth)
            {

                float fintensity = Mathf.InverseLerp(min, max, ir);
                byte intensity = (byte)Mathf.Lerp(255, 0, fintensity);
                _RawData[index++] = intensity;
                _RawData[index++] = intensity;
                _RawData[index++] = intensity;
                _RawData[index++] = 255; // Alpha
            }
            this.texture.LoadRawTextureData(_RawData);
            this.texture.Apply();


            gameObject.renderer.material.mainTexture = this.texture;
        }
    }

    /// <summary>
    /// This is kind of like the Dispose
    /// </summary>
	private void OnApplicationQuit()
	{
		if (depthReader != null)
		{
			depthReader.Dispose();
			depthReader = null;
		}
		
		if (sensor != null)
		{
			if (sensor.IsOpen)
			{
				sensor.Close();
			}
			
			sensor = null;
		}
	}
}
