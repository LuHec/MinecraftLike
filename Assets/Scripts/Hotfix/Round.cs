using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : MonoBehaviour
{
    // 定义旋转速度
    public float rotationSpeed = 90f; // 每秒旋转角度

    void Update()
    {
        Rotate();
        Step();
    }

    private void Rotate()
    {
        // 获取当前旋转
        Quaternion currentRotation = transform.rotation;

        // 定义绕自身up轴的旋转角度
        float angle = rotationSpeed * Time.deltaTime;

        // 创建绕自身up轴的旋转四元数
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);

        // 计算新的旋转四元数
        Quaternion newRotation = currentRotation * rotation;

        // 应用新的旋转
        transform.rotation = newRotation;
    }

    private void Step()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Sin(Time.frameCount) * 2;
        transform.position = pos;
    }
}