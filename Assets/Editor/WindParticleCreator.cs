using UnityEngine;
using UnityEditor;

public class WindParticleCreator : MonoBehaviour
{
    [MenuItem("GameObject/Effects/Wind Distortion (Shader)", false, 12)]
    static void CreateWindDistortion()
    {
        // Создаём Quad для отображения искажения
        GameObject distortionObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        distortionObj.name = "Wind Distortion";
        
        // Удаляем коллайдер (не нужен для визуала)
        Object.DestroyImmediate(distortionObj.GetComponent<Collider>());
        
        // Позиционируем
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            distortionObj.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
        
        // Масштабируем (вытянутый вверх)
        distortionObj.transform.localScale = new Vector3(2f, 4f, 1f);
        
        // Поворачиваем горизонтально (лицом вверх)
        distortionObj.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        
        // Создаём материал с шейдером
        Shader windShader = Shader.Find("Custom/WindDistortion");
        if (windShader != null)
        {
            Material mat = new Material(windShader);
            mat.name = "WindDistortionMaterial";
            mat.SetFloat("_DistortionStrength", 0.015f);
            mat.SetFloat("_Speed", 4f);
            mat.SetFloat("_NoiseScale", 3f);
            
            distortionObj.GetComponent<MeshRenderer>().material = mat;
            
            // Сохраняем материал в папку
            string path = "Assets/Material/WindDistortionMaterial.mat";
            if (!System.IO.Directory.Exists("Assets/Material"))
            {
                System.IO.Directory.CreateDirectory("Assets/Material");
            }
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Wind Distortion создан! Материал сохранён в " + path);
        }
        else
        {
            Debug.LogWarning("Шейдер Custom/WindDistortion не найден! Убедитесь что WindDistortion.shader в папке Shaders");
        }
        
        Selection.activeGameObject = distortionObj;
        Undo.RegisterCreatedObjectUndo(distortionObj, "Create Wind Distortion");
    }
    
    [MenuItem("GameObject/Effects/Fan Complete (Physics + Particles + Distortion)", false, 13)]
    static void CreateCompleteFan()
    {
        // Создаём полный вентилятор со всеми эффектами
        GameObject fanObj = new GameObject("Fan Complete");
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            fanObj.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
        
        // Триггер для физики
        BoxCollider trigger = fanObj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2f, 5f, 2f);
        trigger.center = new Vector3(0f, 2.5f, 0f);
        
        // Скрипт Fan
        fanObj.AddComponent<Fan>();
        
        // --- Частицы ---
        GameObject particlesObj = new GameObject("Wind Particles");
        particlesObj.transform.SetParent(fanObj.transform);
        particlesObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 0.6f;
        main.startSpeed = 10f;
        main.startSize = 0.12f;
        main.startColor = new Color(0.85f, 0.92f, 1f, 0.2f);
        main.maxParticles = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 80f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.6f;
        shape.rotation = new Vector3(-90f, 0f, 0f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.35f, 0.15f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 2f;
        
        var renderer = particlesObj.GetComponent<ParticleSystemRenderer>();
        Material particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMat.SetColor("_Color", new Color(0.85f, 0.92f, 1f, 0.25f));
        renderer.material = particleMat;
        
        // --- Искажение воздуха ---
        GameObject distortionObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        distortionObj.name = "Wind Distortion";
        Object.DestroyImmediate(distortionObj.GetComponent<Collider>());
        distortionObj.transform.SetParent(fanObj.transform);
        distortionObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        distortionObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        distortionObj.transform.localScale = new Vector3(2f, 5f, 1f);
        
        Shader windShader = Shader.Find("Custom/WindDistortion");
        if (windShader != null)
        {
            Material distortMat = new Material(windShader);
            distortMat.SetFloat("_DistortionStrength", 0.012f);
            distortMat.SetFloat("_Speed", 5f);
            distortionObj.GetComponent<MeshRenderer>().material = distortMat;
        }
        
        Selection.activeGameObject = fanObj;
        Undo.RegisterCreatedObjectUndo(fanObj, "Create Complete Fan");
        
        Debug.Log("Полный Fan создан: физика + частицы + шейдер искажения!");
    }

    [MenuItem("GameObject/Effects/Wind Particle System", false, 10)]
    static void CreateWindParticles()
    {
        // Создаём родительский объект
        GameObject windObj = new GameObject("Wind Particles");
        
        // Позиционируем в центре вида сцены
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            windObj.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
        
        // Добавляем Particle System
        ParticleSystem ps = windObj.AddComponent<ParticleSystem>();
        
        // Настраиваем Main модуль
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 8f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.85f, 0.92f, 1f, 0.25f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        
        // Настраиваем Emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 60f;
        
        // Настраиваем Shape (конус вверх)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.8f;
        shape.radiusThickness = 1f;
        shape.rotation = new Vector3(-90f, 0f, 0f); // Направлен вверх
        
        // Настраиваем Size over Lifetime (уменьшается)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, 1.2f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Настраиваем Color over Lifetime (затухание)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.4f, 0.2f),
                new GradientAlphaKey(0.3f, 0.7f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Настраиваем Velocity over Lifetime (небольшое колебание)
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        // Настраиваем Noise (турбулентность)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 1.5f;
        noise.scrollSpeed = 0.5f;
        noise.damping = true;
        
        // Настраиваем Renderer
        var renderer = windObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Создаём материал
        Material windMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        windMaterial.name = "WindParticleMaterial";
        windMaterial.SetColor("_Color", new Color(0.85f, 0.92f, 1f, 0.3f));
        windMaterial.SetFloat("_Mode", 2); // Fade mode
        windMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        windMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        windMaterial.renderQueue = 3000;
        renderer.material = windMaterial;
        
        // Выделяем созданный объект
        Selection.activeGameObject = windObj;
        
        // Регистрируем для Undo
        Undo.RegisterCreatedObjectUndo(windObj, "Create Wind Particles");
        
        Debug.Log("Wind Particle System создана! Настройте направление поворотом объекта или через Shape > Rotation");
    }
    
    [MenuItem("GameObject/Effects/Fan with Wind Effect", false, 11)]
    static void CreateFanWithWind()
    {
        // Создаём родительский объект вентилятора
        GameObject fanObj = new GameObject("Fan");
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            fanObj.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
        
        // Добавляем Box Collider как триггер
        BoxCollider trigger = fanObj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2f, 5f, 2f);
        trigger.center = new Vector3(0f, 2.5f, 0f);
        
        // Добавляем скрипт Fan (если существует)
        var fanScript = fanObj.AddComponent<Fan>();
        
        // Создаём дочерний объект для частиц
        GameObject particlesObj = new GameObject("Wind Particles");
        particlesObj.transform.SetParent(fanObj.transform);
        particlesObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        
        // Настройки как выше
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.6f;
        main.startSpeed = 10f;
        main.startSize = 0.12f;
        main.startColor = new Color(0.85f, 0.92f, 1f, 0.2f);
        main.maxParticles = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 80f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.6f;
        shape.rotation = new Vector3(-90f, 0f, 0f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.35f, 0.15f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 2f;
        
        var renderer = particlesObj.GetComponent<ParticleSystemRenderer>();
        Material windMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        windMaterial.SetColor("_Color", new Color(0.85f, 0.92f, 1f, 0.25f));
        renderer.material = windMaterial;
        
        Selection.activeGameObject = fanObj;
        Undo.RegisterCreatedObjectUndo(fanObj, "Create Fan with Wind");
        
        Debug.Log("Fan с Wind Particles создан! Не забудьте назначить Wind Particles в компоненте Fan.");
    }
}
