#version 430

//#define EPSILON = 0.001
//#define BIG = 1000000.0

#define EPSILON 0.001

const int DIFFUSE = 1;				// рассеивание
const int REFLECTION = 2;			// отражение
const int REFRACTION = 3;			// преломление

const int DIFFUSE_REFLECTION = 1;	// диффузное отражение 
const int MIRROR_REFLECTION = 2;	// зеркальное отражение 

const int MAX_STACK_SIZE = 10;
const int MAX_TRACE_DEPTH = 8;

const vec3 Unit = vec3 ( 1.0, 1.0, 1.0 );
out vec4 FragColor;
in vec3 glPosition;

uniform vec3 camera_position;
uniform vec3 cube_color;
uniform vec3 tetrahedron_color;

uniform float set_reflection_cube;
uniform float set_reflection_tetrahedron;
uniform float set_depth;


// =========================================      Структуры      ========================================= //

struct SCamera 
{
    vec3 Position;
    vec3 View;
    vec3 Up;
    vec3 Side;
    vec2 Scale;			// масштаб
}; 
 
struct SRay 
{
    vec3 Origin;		// источник
	vec3 Direction;		// направление
}; 
 
struct SSphere			
{    
    vec3 Center;
	float Radius;

	int MaterialIdx;	// идекс в массиве материалов
}; 

struct STriangle		
{     
    vec3 v1;   
	vec3 v2;   
	vec3 v3;   

	int MaterialIdx;	
};

struct SMaterial 
{  
    vec3 Color;				// хранение цвета

	vec4 LightCoeffs;		// коэффициенты освещенности	
	float ReflectionCoef;	// коэффициент отражения
	float RefractionCoef;   // коэффициент преломления

	int MaterialType;		// материал текущей точки пересечения
};

struct SCube			
{
	STriangle bounds[12];	// создаем массив из 12 треугольников для отрисовки нашего куба на сцене

	int MaterialIdx;		// идекс в массиве материалов
};

struct STetrahedron
{
	STriangle Tbounds[4];

	int MaterialIdx;
};

struct SIntersection	// структура хранения пересечения
{         
	float Time;

	vec3 Point;				
	vec3 Normal;			
	vec3 Color;				// хранение цвета

	vec4 LightCoeffs;		// коэффициенты освещенности				
	float ReflectionCoef;	// коэффициент отражения
	float RefractionCoef;   // коэффициент преломления
	
	int MaterialType;		// материал текущей точки пересечения
};

struct SLight			// добавление источника света
{ 
    vec3 Position; 
};

struct STracingRay		// отражение луча от объекта
{ 
    SRay ray;				// луч
	float contribution;		// хранение вклада луча в результирующий цвет
	float depth;			// номер переотражения
};

// ========================================= Переменные, массивы ========================================= //

STriangle Triangles[12];	// массив треугольников, из которого построим куб
SSphere Spheres[2];			// массив сфер
SMaterial Materials[9];		// массив материалов

SCube cube;
SLight uLight;				// источник освещения
SCamera uCamera;
STetrahedron tetrahedron;

// =========================================      Функции        ========================================= //

SRay GenerateRay ( SCamera uCamera )	// генерация луча
{  
    vec2 coords = glPosition.xy * uCamera.Scale;
	vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;	// направление
	
	return SRay ( uCamera.Position, normalize(direction) );
}

void initializeDefaultScene (out STriangle triangles[12], out SSphere spheres[2])		// инициализация треугольников, сфер и куба
{
	// ******** инициализация "Треугольник" ********
    triangles[0].v1 = vec3(-5.0,-5.0,-8.0); 
	triangles[0].v2 = vec3(-5.0, 5.0, 5.0); 
	triangles[0].v3 = vec3(-5.0, 5.0,-8.0); 
	triangles[0].MaterialIdx = 0; 
 
    triangles[1].v1 = vec3(-5.0,-5.0,-8.0);
	triangles[1].v2 = vec3(-5.0,-5.0, 5.0);
	triangles[1].v3 = vec3(-5.0, 5.0, 5.0); 
	triangles[1].MaterialIdx = 0;
	
	triangles[2].v1 = vec3(-5.0, 5.0, 5.0); 
	triangles[2].v2 = vec3(-5.0, -5.0, 5.0); 
	triangles[2].v3 = vec3(5.0, -5.0, 5.0); 
	triangles[2].MaterialIdx = 1; 
 
    triangles[3].v1 = vec3(5.0,-5.0, 5.0);
	triangles[3].v2 = vec3(5.0, 5.0, 5.0);
	triangles[3].v3 = vec3(-5.0, 5.0, 5.0); 
	triangles[3].MaterialIdx = 1;
	
	triangles[4].v1 = vec3(5.0, -5.0, 5.0); 
	triangles[4].v2 = vec3(5.0, 5.0, 5.0); 
	triangles[4].v3 = vec3(5.0, 5.0, -8.0); 
	triangles[4].MaterialIdx = 2; 
 
    triangles[5].v1 = vec3(5.0, 5.0,-8.0);
	triangles[5].v2 = vec3(5.0, -5.0, -8.0);
	triangles[5].v3 = vec3(5.0, -5.0, 5.0); 
	triangles[5].MaterialIdx = 2;
	
	triangles[6].v1 = vec3(-5.0, 5.0, 5.0); 
	triangles[6].v2 = vec3(-5.0, 5.0, -8.0); 
	triangles[6].v3 = vec3(5.0, 5.0, -8.0); 
	triangles[6].MaterialIdx = 3; 
 
    triangles[7].v1 = vec3(5.0, 5.0, -8.0); 
	triangles[7].v2 = vec3(5.0, 5.0, 5.0); 
	triangles[7].v3 = vec3(-5.0, 5.0, 5.0); 
	triangles[7].MaterialIdx = 3;
 
    triangles[8].v1 = vec3(-5.0, -5.0, 5.0);
	triangles[8].v2 = vec3(-5.0, -5.0, -8.0);
	triangles[8].v3 = vec3(5.0, -5.0, -8.0); 
	triangles[8].MaterialIdx = 4;
	
	triangles[9].v1 = vec3(5.0,-5.0,-8.0);
	triangles[9].v2 = vec3(5.0, -5.0, 5.0);
	triangles[9].v3 = vec3(-5.0, -5.0, 5.0); 
	triangles[9].MaterialIdx = 4;
	
	triangles[10].v1 = vec3(-5.0, -5.0, -8.0);
	triangles[10].v2 = vec3(5.0, -5.0, -8.0);
	triangles[10].v3 = vec3(5.0, 5.0, -8.0); 
	triangles[10].MaterialIdx = 5;
	
	triangles[11].v1 = vec3(5.0, 5.0,-8.0);
	triangles[11].v2 = vec3(-5.0, 5.0, -8.0);
	triangles[11].v3 = vec3(-5.0, -5.0, -8.0); 
	triangles[11].MaterialIdx = 5;
	
	// ******** инициализация "Сфера" ******** 
	spheres[0].Center = vec3(2.0,0.0,2.0);  
	spheres[0].Radius = 0.3;  
	spheres[0].MaterialIdx = 6; 
 
    spheres[1].Center = vec3(-1.5,1.0,1.0);  
	spheres[1].Radius = 1.3;  
	spheres[1].MaterialIdx = 6;

	// ******** инициализация "Куб" ******** 
	cube.bounds[0].v1 = vec3(1.0,1.0,2.0);
	cube.bounds[0].v2 = vec3(1.0,1.5,2.0);
	cube.bounds[0].v3 = vec3(1.0,1.0,1.5);
	cube.bounds[0].MaterialIdx = 7;
	
	cube.bounds[1].v1 = vec3(1.0,1.5,1.5);
	cube.bounds[1].v2 = vec3(1.0,1.5,2.0);
	cube.bounds[1].v3 = vec3(1.0,1.0,1.5);
	cube.bounds[1].MaterialIdx = 7;

	cube.bounds[2].v1 = vec3(1.0,1.0,2.0);
	cube.bounds[2].v2 = vec3(1.0,1.5,2.0);
	cube.bounds[2].v3 = vec3(1.5,1.0,2.0);
	cube.bounds[2].MaterialIdx = 7;
	
	cube.bounds[3].v1 = vec3(1.5,1.5,2.0);
	cube.bounds[3].v2 = vec3(1.0,1.5,2.0);
	cube.bounds[3].v3 = vec3(1.5,1.0,2.0);
	cube.bounds[3].MaterialIdx = 7;
	
	cube.bounds[4].v1 = vec3(1.5,1.5,1.5);
	cube.bounds[4].v2 = vec3(1.0,1.5,1.5);
	cube.bounds[4].v3 = vec3(1.0,1.5,2.0);
	cube.bounds[4].MaterialIdx = 7;
	
	cube.bounds[5].v1 = vec3(1.5,1.5,1.5);
	cube.bounds[5].v2 = vec3(1.5,1.5,2.0);
	cube.bounds[5].v3 = vec3(1.0,1.5,2.0);
	cube.bounds[5].MaterialIdx = 7;
	
	cube.bounds[6].v1 = vec3(1.5,1.5,1.5);
	cube.bounds[6].v2 = vec3(1.5,1.5,2.0);
	cube.bounds[6].v3 = vec3(1.5,1.0,1.5);
	cube.bounds[6].MaterialIdx = 7;
	
	cube.bounds[7].v1 = vec3(1.5,1.0,2.0);
	cube.bounds[7].v2 = vec3(1.5,1.5,2.0);
	cube.bounds[7].v3 = vec3(1.5,1.0,1.5);
	cube.bounds[7].MaterialIdx = 7;
	
	cube.bounds[8].v1 = vec3(1.0,1.0,2.0);
	cube.bounds[8].v2 = vec3(1.0,1.0,1.5);
	cube.bounds[8].v3 = vec3(1.5,1.0,1.5);
	cube.bounds[8].MaterialIdx = 7;

	cube.bounds[9].v1 = vec3(1.0,1.0,2.0);
	cube.bounds[9].v2 = vec3(1.5,1.0,2.0);
	cube.bounds[9].v3 = vec3(1.5,1.0,1.5);
	cube.bounds[9].MaterialIdx = 7;
	
	cube.bounds[10].v1 = vec3(1.0,1.5,1.5);
	cube.bounds[10].v2 = vec3(1.5,1.5,1.5);
	cube.bounds[10].v3 = vec3(1.0,1.0,1.5);
	cube.bounds[10].MaterialIdx = 7;
	
	cube.bounds[11].v1 = vec3(1.5,1.0,1.5);
	cube.bounds[11].v2 = vec3(1.5,1.5,1.5);
	cube.bounds[11].v3 = vec3(1.0,1.0,1.5);
	cube.bounds[11].MaterialIdx = 7;
	cube.MaterialIdx = 7;

	// ******** инициализация "Тетраэдер" ********
	tetrahedron.Tbounds[0].v1 = vec3(1.0,-2.0,3.0);		
	tetrahedron.Tbounds[0].v2 = vec3(1.0,-3.5,3.0);		
	tetrahedron.Tbounds[0].v3 = vec3(3.5,-2.0,3.0);		
	tetrahedron.Tbounds[0].MaterialIdx = 8;

	tetrahedron.Tbounds[1].v1 = vec3(1.5,-2.5,2.5);		
	tetrahedron.Tbounds[1].v2 = vec3(1.0,-2.0,3.0);		
	tetrahedron.Tbounds[1].v3 = vec3(3.5,-2.0,3.0);		
	tetrahedron.Tbounds[1].MaterialIdx = 8;
	
	tetrahedron.Tbounds[2].v1 = vec3(1.5,-2.5,2.5);		
	tetrahedron.Tbounds[2].v2 = vec3(1.0,-3.5,3.0);		
	tetrahedron.Tbounds[2].v3 = vec3(3.5,-2.0,3.0);		
	tetrahedron.Tbounds[2].MaterialIdx = 8;

	tetrahedron.Tbounds[3].v1 = vec3(1.5,-2.5,2.5);		
	tetrahedron.Tbounds[3].v2 = vec3(1.0,-2.0,3.0);		
	tetrahedron.Tbounds[3].v3 = vec3(1.0,-3.5,3.0);		
	tetrahedron.Tbounds[3].MaterialIdx = 8;
	
	tetrahedron.MaterialIdx = 8;
}

void initializeDefaultLightMaterials(out SLight light, out SMaterial materials[9]) 
{
    light.Position = vec3(0.0, 3.0, -4.0f);				// позиция иточника света
 
    vec4 lightCoefs = vec4(0.4,0.9,0.25,10.0);			// настройки источника света (чтобы включить отображение точки, где источник света,
														// нужно поменять третью переменную)
	materials[0].Color = vec3(0.0, 1.0, 0.0);			// цвет стены сцены слева
	materials[0].LightCoeffs = vec4(lightCoefs);
	materials[0].ReflectionCoef = 0.5;   
	materials[0].RefractionCoef = 1.0;  
	materials[0].MaterialType = DIFFUSE_REFLECTION;		
 
    materials[1].Color = vec3(0.1, 0.1, 0.1);			// цвет стены сцены спереди
	materials[1].LightCoeffs = vec4(lightCoefs); 
    materials[1].ReflectionCoef = 0.5;  
	materials[1].RefractionCoef = 1.0;  
	materials[1].MaterialType = DIFFUSE_REFLECTION;
	
	materials[2].Color = vec3(1.0, 0.0, 0.0);			// цвет стены сцены справа
	materials[2].LightCoeffs = vec4(lightCoefs); 
    materials[2].ReflectionCoef = 0.5;  
	materials[2].RefractionCoef = 1.0;  
	materials[2].MaterialType = DIFFUSE_REFLECTION;		
	
	materials[3].Color = vec3(1.0, 1.0, 0.0);			// цвет стены сцены сверху
	materials[3].LightCoeffs = vec4(lightCoefs); 
    materials[3].ReflectionCoef = 0.5;  
	materials[3].RefractionCoef = 1.0;  
	materials[3].MaterialType = MIRROR_REFLECTION;
	
	materials[4].Color = vec3(0.0, 1.0, 1.0);			// цвет стены сцены снизу
	materials[4].LightCoeffs = vec4(lightCoefs); 
    materials[4].ReflectionCoef = 0.5;  
	materials[4].RefractionCoef = 1.0;  
	materials[4].MaterialType = DIFFUSE_REFLECTION;
	
	materials[5].Color = vec3(0.0, 0.0, 1.0);			// цвет стены сцены сзади
	materials[5].LightCoeffs = vec4(lightCoefs); 
    materials[5].ReflectionCoef = 0.5;  
	materials[5].RefractionCoef = 1.0;  
	materials[5].MaterialType = DIFFUSE_REFLECTION;
	
	materials[6].Color = vec3(1.0, 1.0, 1.0);			// цвет отражения от сфер
	materials[6].LightCoeffs = vec4(lightCoefs); 
    materials[6].ReflectionCoef = 0.5;  
	materials[6].RefractionCoef = 1.0;  
	materials[6].MaterialType = MIRROR_REFLECTION;

	if (set_reflection_cube == 1)						// цвет куба
	{
		materials[7].Color = cube_color;					
		materials[7].LightCoeffs = vec4(lightCoefs);  
		materials[7].ReflectionCoef = 0.5;  
		materials[7].RefractionCoef = 1.0;  
		materials[7].MaterialType = MIRROR_REFLECTION;
	}
	else
	{
		materials[7].Color = cube_color;					
		materials[7].LightCoeffs = vec4(lightCoefs);  
		materials[7].ReflectionCoef = 0.5;  
		materials[7].RefractionCoef = 1.0;  
		materials[7].MaterialType = DIFFUSE_REFLECTION;
	}
				
	materials[8].Color = vec3(0.2, 0.2, 0.2);				// цвет тетраэдра
	materials[8].LightCoeffs = vec4(lightCoefs); 
	materials[8].ReflectionCoef = 0.0;  
	materials[8].RefractionCoef = 1.3;  
	materials[8].MaterialType = REFRACTION;
}

bool IntersectSphere ( SSphere sphere, SRay ray, float start, float final, out float time )		// пересечение луча со сферой
{     
    ray.Origin -= sphere.Center;													// Origin - откуда исходит луч
	
	float A = dot ( ray.Direction, ray.Direction );									// Direction - направление луча
	float B = dot ( ray.Direction, ray.Origin );   
	float C = dot ( ray.Origin, ray.Origin ) - sphere.Radius * sphere.Radius;		// (уравнение луча)^2 - R^2
	float D = B * B - A * C;														// дискриминант

    if ( D > 0.0 )  
	{
    	D = sqrt ( D );

		float t1 = ( -B - D ) / A;   
		float t2 = ( -B + D ) / A;      

		if(t1 < 0 && t2 < 0)
		{
			return false; 
		} 
		
        if(min(t1, t2) < 0)   
		{            
    		time = max(t1,t2);      
			return true;      
		}  

		time = min(t1, t2);    

		return true;  
	}  
	return false; 
}

bool IntersectTriangle (SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time )						// пересечение луча с треугольником
{
    time = -1; 

	vec3 A = v2 - v1;										// на вход: v1, v2, v3 - вершины треугольника
	vec3 B = v3 - v1; 	
	vec3 N = cross(A, B);									// вычисление вектрного произведение двух векторов

	float NdotRayDirection = dot(N, ray.Direction);			

	if (abs(NdotRayDirection) < 0.001)						// проверка: параллельны ли луч и плоскость?
	{
		return false; 
	}
												// ?? продолжаем, если они парраллельны ??
	float d = dot(N, v1);
	float t = -(dot(N, ray.Origin) - d) / NdotRayDirection; 

	if (t < 0)												// проверка: находится ли треугольник за камерой?
	{														
		return false; 
	}
															// продолжаем, если треугольник находится перед камерой
	vec3 P = ray.Origin + t * ray.Direction;				// вычисляем точку пересечения
	vec3 C;
	
	vec3 edge1 = v2 - v1;									// край 1
	vec3 VP1 = P - v1; 

	C = cross(edge1, VP1); 

	if (dot(N, C) < 0)
	{
		return false;
	}

	vec3 edge2 = v3 - v2;									// край 2
	vec3 VP2 = P - v2; 

	C = cross(edge2, VP2); 

	if (dot(N, C) < 0)  
	{
		return false;
	}

	vec3 edge3 = v1 - v3;									// край 3
	vec3 VP3 = P - v3; 

	C = cross(edge3, VP3); 

	if (dot(N, C) < 0)   
	{
		return false;
	}

	time = t; 

	return true;											// значит, что этот луч попадает в треугольник
}


bool Raytrace ( SRay ray, float start, float final, inout SIntersection intersect )		// пересекает луч со всеми примитивами сцены 
{																						// и возвращает ближайшее пересечение
    bool result = false; 
	float test = start; 
	intersect.Time = final; 
	
	for(int i = 0; i < 12; i++)				// вычисление пересечений с треугольниками
	{
	    STriangle triangle = Triangles[i]; 

	    if(IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test) && test < intersect.Time)
	    {        
    	    intersect.Time = test;  
			
			intersect.Point = ray.Origin + ray.Direction * test;  
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			
			SMaterial mat = Materials[i / 2];
			
			intersect.Color = mat.Color;    
			
			intersect.LightCoeffs = mat.LightCoeffs;
			intersect.ReflectionCoef = mat.ReflectionCoef;       
			intersect.RefractionCoef = mat.RefractionCoef;       
			
			intersect.MaterialType = mat.MaterialType;       
			
			result = true;   
		} 
	}
	
	for(int i = 0; i < 2; i++)				// вычисление пересечений со сферами
	{   
	    SSphere sphere = Spheres[i];

		if( IntersectSphere (sphere, ray, start, final, test ) && test < intersect.Time )  
		{       
    		intersect.Time = test;    
			
			intersect.Point = ray.Origin + ray.Direction * test;      
			intersect.Normal = normalize ( intersect.Point - sphere.Center );
			
			SMaterial mat = Materials[6];
			
			intersect.Color = mat.Color;        
			
			intersect.LightCoeffs = mat.LightCoeffs;
			intersect.ReflectionCoef = mat.ReflectionCoef;   
			intersect.RefractionCoef = mat.RefractionCoef;       
			
			intersect.MaterialType =   mat.MaterialType;  
			
			result = true;    
	    } 
	}

	for(int i = 0; i < 12; i++) 			// вычисление пересечений с кубом
	{
	    STriangle triangle = cube.bounds[i]; 

	    if(IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test) && test < intersect.Time)
	    {        
    	    intersect.Time = test;  
			
			intersect.Point = ray.Origin + ray.Direction * test;  
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			
			SMaterial mat = Materials[7];
			
			intersect.Color = cube_color;
			
			intersect.LightCoeffs = mat.LightCoeffs;
			intersect.ReflectionCoef = mat.ReflectionCoef;       
			intersect.RefractionCoef = mat.RefractionCoef;       
			
			intersect.MaterialType = mat.MaterialType;       
			
			result = true;   
		} 
	}

	for(int i = 0; i < 4; i++) 			// вычисление пересечений с тетраэдром
	{
	    STriangle triangle = tetrahedron.Tbounds[i]; 

	    if(IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test) && test < intersect.Time)
	    {        
    	    intersect.Time = test;  
			
			intersect.Point = ray.Origin + ray.Direction * test;  
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			
			SMaterial mat = Materials[8];
			
			intersect.Color = mat.Color; 
			
			intersect.LightCoeffs = mat.LightCoeffs;
			intersect.ReflectionCoef = mat.ReflectionCoef;       
			intersect.RefractionCoef = mat.RefractionCoef;       
			
			intersect.MaterialType = mat.MaterialType;       
			
			result = true;   
		} 
	}
	return result;
} 

vec3 Phong ( SIntersection intersect, SLight currLight, float shadowing)				// модель освещения по Фонгу
{
    vec3 light = normalize ( currLight.Position - intersect.Point ); 
    
	float diffuse = max(dot(light, intersect.Normal), 0.0);   
	
	vec3 view = normalize(uCamera.Position - intersect.Point);  
	vec3 reflected= reflect( -view, intersect.Normal );   
	
	float specular = pow(max(dot(reflected, light), 0.0), intersect.LightCoeffs.w);    
	
	return intersect.LightCoeffs.x * intersect.Color + 
	       intersect.LightCoeffs.y * diffuse * intersect.Color * shadowing + 
		   intersect.LightCoeffs.z * specular * Unit;
} 

float Shadow(SLight currLight, SIntersection intersect)							// отрисовка теней
{     
    float shadowing = 1.0;  

	vec3 direction = normalize(currLight.Position - intersect.Point);			// вектор к источнику света
	float distanceLight = distance(currLight.Position, intersect.Point);		// расстояние до источника света
	
	SRay shadowRay = SRay(intersect.Point + direction * 0.001, direction);		// генерация теневого луча для этого источника света
	
	SIntersection shadowIntersect;     
	shadowIntersect.Time = 1000000.0;      
	
	if(Raytrace(shadowRay, 0, distanceLight, shadowIntersect))					// трассировка луча от начала теневого луча
	{																			// до положения источника света
    	shadowing = 0.0;     
	}

	return shadowing; 
}

STracingRay stack[MAX_STACK_SIZE];
int stackSize = 0;

bool pushRay(STracingRay secondaryRay)		// кладем луч на стек
{
	if(stackSize < MAX_STACK_SIZE - 1 && secondaryRay.depth < MAX_TRACE_DEPTH)
	{
		stack[stackSize] = secondaryRay;
		stackSize++;
		return true;
	}
	return false;
}

STracingRay popRay()			// берем луч со стека
{
	stackSize--;
	return stack[stackSize];	
}

bool isEmpty()					// проверяем есть ли ещё эл-ты в стеке
{
	if(stackSize < 0)
		return true;
	return false;
}


void main ( void )
{
    float start = 0;   
	float final = 1000000.0;
	
	uCamera.Position = camera_position;
    uCamera.View = vec3(0.0, 0.0, 1.0); 
	uCamera.Up = vec3(0.0, 1.0, 0.0);  
	uCamera.Side = vec3(1.0, 0.0, 0.0); 
	uCamera.Scale = vec2(1.0); 

	SRay ray = GenerateRay(uCamera);
	SIntersection intersect;        

	intersect.Time = 1000000.0;    
	vec3 resultColor = vec3(0,0,0);

	initializeDefaultLightMaterials(uLight, Materials);
    initializeDefaultScene(Triangles, Spheres);	

	STracingRay trRay = STracingRay(ray, 1, set_depth); 
	pushRay(trRay); 

	while(!isEmpty()) 
	{     
	    STracingRay trRay = popRay();     
		ray = trRay.ray;    
		
		SIntersection intersect;  
		intersect.Time = 1000000.0;   
		
		start = 0;     
		final = 1000000.0;    
		
		if (Raytrace(ray, start, final, intersect))
		{   
    		switch(intersect.MaterialType){
    			case DIFFUSE_REFLECTION:         
				{  
    				float shadowing = Shadow(uLight, intersect);   
					
					resultColor += trRay.contribution * Phong ( intersect, uLight, shadowing );   
					
					break;       
				}  
				case MIRROR_REFLECTION: 
				{ 
    				if(intersect.ReflectionCoef < 1)   
					{              
					    float contribution = trRay.contribution * (1 - intersect.ReflectionCoef);     
					    float shadowing = Shadow(uLight, intersect);              
					    
						resultColor +=  contribution * Phong(intersect, uLight, shadowing);    
				    }  
				    vec3 reflectDirection = reflect(ray.Direction, intersect.Normal);
				    
					float contribution = trRay.contribution * intersect.ReflectionCoef;  
				    
					STracingRay reflectRay = STracingRay( SRay(intersect.Point + reflectDirection * 0.001, reflectDirection), 
														  contribution, trRay.depth + 1);    
				    pushRay(reflectRay);  
				    
					break;  
			    }     
				case REFRACTION:
				{
					float diffuse_contrib = trRay.contribution; 
					float shadowing = Shadow(uLight, intersect);
					
					resultColor += diffuse_contrib * Phong(intersect, uLight, shadowing);
					
					if (trRay.depth >= MAX_TRACE_DEPTH)
					{
						break;
					}
					
					float eta = 1.0f / 1.5f;				// (n1 / n2), где n1 - показатель преломления воздуха
															//				  n2 - показатель преломления стекла	
					if (dot(ray.Direction, intersect.Normal) >= 0.0f) 
					{
						intersect.Normal = -intersect.Normal;
						eta = intersect.RefractionCoef / 1.0f;
					}
					
					float refract_contrib = trRay.contribution * intersect.RefractionCoef;		// вклад преломляющего луча в результирующий цвет
					if (refract_contrib > 0.001) 
					{
						vec3 refractDirection = refract(ray.Direction, intersect.Normal, eta);	// вычисление вектора преломления
					
						STracingRay refractRay = STracingRay(SRay(intersect.Point + refractDirection * 0.001, refractDirection), 
																refract_contrib, trRay.depth + 1);	// создание луча преломления
						pushRay(refractRay);
					}
					break;

					//float n1 = 1.0;
					//float n2 = 1.5;
					//vec3 cosB = sqrt((1 - ( n1 / n2 ) * ( n1 / n2)) * 
					//				 (1 - (intersect.Normal * trRay.contribution)*(intersect.Normal * trRay.contribution)));
					//vec3 t = n1/n2 * trRay.contribution - (cosB + (n1/n2 * intersect.Normal * trRay.contribution)*intersect.Normal);
					//
					//float refract_contrib = trRay.contribution * 1.5;
					//
					//if (refract_contrib > 0.001) 
					//{			
					//	STracingRay refractRay = STracingRay(SRay(intersect.Point + t * 0.001, t), 
					//											refract_contrib, trRay.depth + 1);
					//	pushRay(refractRay);
					//}
				}
			}  
		}
	}
    FragColor = vec4(resultColor, 1.0);
}