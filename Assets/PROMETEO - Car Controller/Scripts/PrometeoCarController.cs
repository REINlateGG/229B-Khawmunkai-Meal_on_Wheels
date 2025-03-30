/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))] // เพิ่ม RequireComponent เพื่อความแน่ใจ
public class PrometeoCarController : MonoBehaviour
{

    //CAR SETUP
    [Space(20)]
    [Header("CAR SETUP")]
    [Space(10)]
    [Range(20, 190)]
    public int maxSpeed = 90;
    [Range(10, 120)]
    public int maxReverseSpeed = 45;
    [Range(1, 10)]
    public int accelerationMultiplier = 2;
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5;
    [Space(10)]
    public Vector3 bodyMassCenter;

    //WHEELS
    [Space(20)]
    [Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    // --- เพิ่มตัวแปรสำหรับ Surface Interaction ---
    [Space(20)]
    [Header("SURFACE INTERACTION")]
    [Tooltip("Physic Material สำหรับพื้นผิวปกติ (Optional Reference)")]
    [SerializeField] private PhysicsMaterial normalGroundMaterial;
    [Tooltip("Physic Material สำหรับพื้นผิวลื่น (เช่น คราบน้ำมัน)")]
    [SerializeField] private PhysicsMaterial slipperyMaterial; // Oil / Wet Surface
    [Tooltip("Physic Material สำหรับพื้นผิวน้ำแข็ง")]
    [SerializeField] private PhysicsMaterial iceMaterial;      // Ice
    [Tooltip("Physic Material สำหรับพื้นผิวโคลน")]
    [SerializeField] private PhysicsMaterial mudMaterial;      // Mud
    [Tooltip("Physic Material สำหรับพื้นผิวทราย")]
    [SerializeField] private PhysicsMaterial sandMaterial;     // Sand
    [Space(5)]
    [Tooltip("ตัวคูณ Stiffness เมื่ออยู่บนพื้นผิวลื่น (e.g., 0.1 = very slippery)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float slipperyFrictionStiffnessMultiplier = 0.1f;
    [Tooltip("ตัวคูณ Stiffness เมื่ออยู่บนพื้นผิวน้ำแข็ง (e.g., 0.05 = extremely slippery)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float iceFrictionStiffnessMultiplier = 0.05f;
    [Tooltip("ตัวคูณ Stiffness เมื่ออยู่บนพื้นผิวโคลน (e.g., 0.5 = sluggish)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float mudFrictionStiffnessMultiplier = 0.5f;
     [Tooltip("ตัวคูณ Stiffness เมื่ออยู่บนพื้นผิวทราย (e.g., 0.8 = slightly loose)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float sandFrictionStiffnessMultiplier = 0.8f;

    // ตัวแปร private สำหรับเก็บค่า Stiffness ดั้งเดิม
    private float originalFLFwdStiffness, originalFLSideStiffness;
    private float originalFRFwdStiffness, originalFRSideStiffness;
    private float originalRLFwdStiffness, originalRLSideStiffness;
    private float originalRRFwdStiffness, originalRRSideStiffness;
    // ----------------------------------------------

    //PARTICLE SYSTEMS
    [Space(20)]
    [Header("EFFECTS")]
    [Space(10)]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)
    [Space(20)]
    [Header("UI")]
    [Space(10)]
    public bool useUI = false;
    public Text carSpeedText;

    //SOUNDS
    [Space(20)]
    [Header("Sounds")]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //CONTROLS
    [Space(20)]
    [Header("CONTROLS")]
    [Space(10)]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    PrometeoTouchInput throttlePTI;
    public GameObject reverseButton;
    PrometeoTouchInput reversePTI;
    public GameObject turnRightButton;
    PrometeoTouchInput turnRightPTI;
    public GameObject turnLeftButton;
    PrometeoTouchInput turnLeftPTI;
    public GameObject handbrakeButton;
    PrometeoTouchInput handbrakePTI;

    //CAR DATA
    [HideInInspector]
    public float carSpeed;
    [HideInInspector]
    public bool isDrifting;
    [HideInInspector]
    public bool isTractionLocked;

    //PRIVATE VARIABLES
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;
    bool touchControlsSetup = false;
    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        // เก็บค่า Friction เริ่มต้นสำหรับการดริฟต์ (โค้ดเดิม)
        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        // --- เก็บค่า Stiffness ดั้งเดิมของล้อ (เพิ่มเข้ามาใหม่) ---
        StoreOriginalWheelStiffness();
        // ------------------------------------

        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if (!useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = "0";
            }
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null) { RLWParticleSystem.Stop(); }
            if (RRWParticleSystem != null) { RRWParticleSystem.Stop(); }
            if (RLWTireSkid != null) { RLWTireSkid.emitting = false; }
            if (RRWTireSkid != null) { RRWTireSkid.emitting = false; }
        }

        if (useTouchControls)
        {
            // ... (โค้ด Touch Controls เดิม) ...
             if(throttleButton != null && reverseButton != null &&
             turnRightButton != null && turnLeftButton != null
             && handbrakeButton != null){

               throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
               reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
               turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
               turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
               handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
               touchControlsSetup = true;

             }else{
               String ex = "Touch controls are not completely set up. You must drag and drop your scene buttons in the" +
               " PrometeoCarController component.";
               Debug.LogWarning(ex);
             }
        }
    }

    // --- ฟังก์ชันใหม่สำหรับเก็บค่า Stiffness ดั้งเดิม ---
    void StoreOriginalWheelStiffness()
    {
        originalFLFwdStiffness = frontLeftCollider.forwardFriction.stiffness;
        originalFLSideStiffness = frontLeftCollider.sidewaysFriction.stiffness;
        originalFRFwdStiffness = frontRightCollider.forwardFriction.stiffness;
        originalFRSideStiffness = frontRightCollider.sidewaysFriction.stiffness;
        originalRLFwdStiffness = rearLeftCollider.forwardFriction.stiffness;
        originalRLSideStiffness = rearLeftCollider.sidewaysFriction.stiffness;
        originalRRFwdStiffness = rearRightCollider.forwardFriction.stiffness;
        originalRRSideStiffness = rearRightCollider.sidewaysFriction.stiffness;
    }
    // -------------------------------------------------

    void Update()
    {
        //CAR DATA
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // --- เพิ่มส่วนนี้: อัปเดต Friction ของล้อตามพื้นผิว ---
        UpdateWheelFrictionBasedOnSurface();
        // --------------------------------------------------

        //CAR PHYSICS (Input Handling)
        if (useTouchControls && touchControlsSetup)
        {
            // ... (โค้ด Touch Input Handling เดิม) ...
              if(throttlePTI.buttonPressed){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoForward();
              }
              if(reversePTI.buttonPressed){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoReverse();
              }

              if(turnLeftPTI.buttonPressed){
                TurnLeft();
              }
              if(turnRightPTI.buttonPressed){
                TurnRight();
              }
              if(handbrakePTI.buttonPressed){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
              }
              if(!handbrakePTI.buttonPressed){
                RecoverTraction();
              }
              if((!throttlePTI.buttonPressed && !reversePTI.buttonPressed)){
                ThrottleOff();
              }
              if((!reversePTI.buttonPressed && !throttlePTI.buttonPressed) && !handbrakePTI.buttonPressed && !deceleratingCar){
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
              }
              if(!turnLeftPTI.buttonPressed && !turnRightPTI.buttonPressed && steeringAxis != 0f){
                ResetSteeringAngle();
              }
        }
        else
        {
            // ... (โค้ด Keyboard Input Handling เดิม) ...
              if(Input.GetKey(KeyCode.W)){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoForward();
              }
              if(Input.GetKey(KeyCode.S)){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                GoReverse();
              }

              if(Input.GetKey(KeyCode.A)){
                TurnLeft();
              }
              if(Input.GetKey(KeyCode.D)){
                TurnRight();
              }
              if(Input.GetKey(KeyCode.Space)){
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
              }
              if(Input.GetKeyUp(KeyCode.Space)){
                RecoverTraction();
              }
              if((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))){
                ThrottleOff();
              }
              if((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) && !Input.GetKey(KeyCode.Space) && !deceleratingCar){
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
              }
              if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f){
                ResetSteeringAngle();
              }
        }

        // Animate Wheel Meshes
        AnimateWheelMeshes();
    }

     // --- ฟังก์ชันใหม่: ตรวจสอบและปรับ Friction ของล้อทั้งหมด ---
    void UpdateWheelFrictionBasedOnSurface()
    {
        CheckAndAdjustFrictionForWheel(frontLeftCollider, originalFLFwdStiffness, originalFLSideStiffness);
        CheckAndAdjustFrictionForWheel(frontRightCollider, originalFRFwdStiffness, originalFRSideStiffness);
        CheckAndAdjustFrictionForWheel(rearLeftCollider, originalRLFwdStiffness, originalRLSideStiffness);
        CheckAndAdjustFrictionForWheel(rearRightCollider, originalRRFwdStiffness, originalRRSideStiffness);
    }

    // --- ฟังก์ชันช่วย: ตรวจสอบพื้นผิวใต้ล้อเดียวและปรับ Stiffness ---
    void CheckAndAdjustFrictionForWheel(WheelCollider wheel, float originalFwdStiffness, float originalSideStiffness)
    {
        WheelFrictionCurve fwdFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        if (wheel.GetGroundHit(out WheelHit hit))
        {
            PhysicsMaterial groundMat = hit.collider.sharedMaterial;

            if (groundMat == slipperyMaterial) // เช็คน้ำมัน
            {
                fwdFriction.stiffness = originalFwdStiffness * slipperyFrictionStiffnessMultiplier;
                sidewaysFriction.stiffness = originalSideStiffness * slipperyFrictionStiffnessMultiplier;
            }
            else if (groundMat == iceMaterial) // เช็คน้ำแข็ง
            {
                 fwdFriction.stiffness = originalFwdStiffness * iceFrictionStiffnessMultiplier;
                 sidewaysFriction.stiffness = originalSideStiffness * iceFrictionStiffnessMultiplier;
            }
             else if (groundMat == mudMaterial) // เช็คโคลน
            {
                fwdFriction.stiffness = originalFwdStiffness * mudFrictionStiffnessMultiplier;
                sidewaysFriction.stiffness = originalSideStiffness * mudFrictionStiffnessMultiplier;
            }
            else if (groundMat == sandMaterial) // เช็คทราย
            {
                 fwdFriction.stiffness = originalFwdStiffness * sandFrictionStiffnessMultiplier;
                 sidewaysFriction.stiffness = originalSideStiffness * sandFrictionStiffnessMultiplier;
            }
            else // พื้นผิวปกติ (หรือ Material อื่นๆ ที่ไม่ได้ระบุ)
            {
                fwdFriction.stiffness = originalFwdStiffness;
                sidewaysFriction.stiffness = originalSideStiffness;
            }
        }
        else // ล้อลอย
        {
             fwdFriction.stiffness = originalFwdStiffness;
             sidewaysFriction.stiffness = originalSideStiffness;
        }

        // กำหนดค่ากลับเข้าไป
        wheel.forwardFriction = fwdFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }
    // -------------------------------------------------------------


    // --- โค้ดส่วนที่เหลือ (เหมือนเดิมทุกประการ) ---

    public void CarSpeedUI(){
      // ... (โค้ดเดิม) ...
        if(useUI){
            try{
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }catch(Exception ex){
                Debug.LogWarning(ex);
            }
        }
    }

    public void CarSounds(){
        // ... (โค้ดเดิม) ...
        if(useSounds){
            try{
                if(carEngineSound != null){
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }
                if((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f)){
                    if(!tireScreechSound.isPlaying){
                        tireScreechSound.Play();
                    }
                }else if((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f)){
                    tireScreechSound.Stop();
                }
            }catch(Exception ex){
                Debug.LogWarning(ex);
            }
        }else if(!useSounds){
            if(carEngineSound != null && carEngineSound.isPlaying){
                carEngineSound.Stop();
            }
            if(tireScreechSound != null && tireScreechSound.isPlaying){
                tireScreechSound.Stop();
            }
        }
    }

    public void TurnLeft(){
        // ... (โค้ดเดิม) ...
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if(steeringAxis < -1f){
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight(){
        // ... (โค้ดเดิม) ...
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if(steeringAxis > 1f){
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle(){
        // ... (โค้ดเดิม) ...
        if(steeringAxis < 0f){
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }else if(steeringAxis > 0f){
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        if(Mathf.Abs(frontLeftCollider.steerAngle) < 1f){
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    void AnimateWheelMeshes(){
        // ... (โค้ดเดิม) ...
        try{
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }catch(Exception ex){
            Debug.LogWarning(ex);
        }
    }

    public void GoForward(){
        // ... (โค้ดเดิม) ...
        if(Mathf.Abs(localVelocityX) > 2.5f){
            isDrifting = true;
            DriftCarPS();
        }else{
            isDrifting = false;
            DriftCarPS();
        }
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if(throttleAxis > 1f){
            throttleAxis = 1f;
        }
        if(localVelocityZ < -1f){
            Brakes();
        }else{
            if(Mathf.RoundToInt(carSpeed) < maxSpeed){
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }else {
                        frontLeftCollider.motorTorque = 0;
                        frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                        rearRightCollider.motorTorque = 0;
                }
        }
    }

    public void GoReverse(){
        // ... (โค้ดเดิม) ...
        if(Mathf.Abs(localVelocityX) > 2.5f){
            isDrifting = true;
            DriftCarPS();
        }else{
            isDrifting = false;
            DriftCarPS();
        }
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if(throttleAxis < -1f){
            throttleAxis = -1f;
        }
        if(localVelocityZ > 1f){
            Brakes();
        }else{
            if(Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed){
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }else {
                        frontLeftCollider.motorTorque = 0;
                        frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                        rearRightCollider.motorTorque = 0;
                }
        }
    }

    public void ThrottleOff(){
        // ... (โค้ดเดิม) ...
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar(){
        // ... (โค้ดเดิม) ...
        if(Mathf.Abs(localVelocityX) > 2.5f){
            isDrifting = true;
            DriftCarPS();
        }else{
            isDrifting = false;
            DriftCarPS();
        }
        if(throttleAxis != 0f){
            if(throttleAxis > 0f){
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }else if(throttleAxis < 0f){
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if(Mathf.Abs(throttleAxis) < 0.15f){
                throttleAxis = 0f;
            }
        }
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        if(carRigidbody.linearVelocity.magnitude < 0.25f){
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes(){
        // ... (โค้ดเดิม) ...
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake(){
        // ... (โค้ดเดิม) ...
        CancelInvoke("RecoverTraction");
        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if(secureStartingPoint < FLWextremumSlip){
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        }
        if(driftingAxis > 1f){
            driftingAxis = 1f;
        }
        if(Mathf.Abs(localVelocityX) > 2.5f){
            isDrifting = true;
        }else{
            isDrifting = false;
        }
        if(driftingAxis < 1f){
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }
        isTractionLocked = true;
        DriftCarPS();
    }

    public void DriftCarPS(){
        // ... (โค้ดเดิม) ...
        if(useEffects){
            try{
                if(isDrifting){
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }else if(!isDrifting){
                    RLWParticleSystem.Stop();
                    RRWParticleSystem.Stop();
                }
            }catch(Exception ex){
                Debug.LogWarning(ex);
            }

            try{
                if((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f){
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }else {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }catch(Exception ex){
                Debug.LogWarning(ex);
            }
        }else if(!useEffects){
            if(RLWParticleSystem != null){RLWParticleSystem.Stop();}
            if(RRWParticleSystem != null){RRWParticleSystem.Stop();}
            if(RLWTireSkid != null){RLWTireSkid.emitting = false;}
            if(RRWTireSkid != null){RRWTireSkid.emitting = false;}
        }
    }

    public void RecoverTraction(){
        // ... (โค้ดเดิม) ...
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if(driftingAxis < 0f){
            driftingAxis = 0f;
        }

        if(FLwheelFriction.extremumSlip > FLWextremumSlip){
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);

        }else if (FLwheelFriction.extremumSlip < FLWextremumSlip){
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }

} // End of Class

