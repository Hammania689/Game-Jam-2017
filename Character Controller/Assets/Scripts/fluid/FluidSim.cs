using UnityEngine;
using System.Collections;
using System;

// Fluid Sim for the Propulsion Flames

public class FluidSim : MonoBehaviour
{
	public float dt = 0.8f;
	public float visc = 0;
	public float volatilize = 0;
	public int iterations = 6;
	public Texture2D border;
	public Texture2D flow;
	
	Texture2D tex;
	int width, height;
	float[] u, v, u_prev, v_prev;
	float[] dens, dens_prev;
	float[] bndX, bndY;
	float[] velX, velY;
	int rowSize;
	int size;

	private int num_frame = 0;

	public ComputeShader shader;
	private int kernel_id;
	private ComputeBuffer buf_x_in, buf_x_out, buf_x0, buf_args;

	private Color[] color_buffer = null;

	bool is_collided = false;
	int fire_life = 0;
	int fade_life = 0;

	public enum JetState{
		NORMAL,
		JUMPING
	};
	public JetState state = JetState.NORMAL;

	// For the propulsion flame
	// When hit something fade fire
	void OnTriggerEnter (Collider other) {
		fade_life = 8;
		is_collided = true;
	}

	void OnTriggerStay (Collider other) {
		fade_life ++;
		is_collided = true;
	}

	void OnTriggerExit (Collider other) {
		is_collided = false;
	}

	public void OnCapsuleJumped() {
		fire_life = 22;
	}

	void Start()
	{
		// duplicate the original texture and assign to the material
		/*
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex;
		// get grid dimensions from texture
		width = tex.width;
		height = tex.height;
		*/
		width = 48; height = 64;
		tex = new Texture2D (width, height, TextureFormat.ARGB32, false);
		GetComponent<Renderer> ().material.mainTexture = tex;
		Debug.Log (string.Format("{0}x{1}", width, height));
		// initialize fluid arrays
		rowSize = width + 2;
		size = (width+2)*(height+2);
		dens = new float[size];
		dens_prev = new float[size];
		u = new float[size];
		u_prev = new float[size];
		v = new float[size];
		v_prev = new float[size];
		bndX = new float[size];
		bndY = new float[size];
		velX = new float[size];
		velY = new float[size];
		for (int i = 0; i < size; i++) {
			dens_prev[i] = u_prev[i] = v_prev[i] = dens[i] = u[i] = v[i] = 0;
			int y = i / rowSize;
			int x = i % rowSize;

			if (y >= height || y <= 1) {
				bndX [i] = border.GetPixel (x, y).grayscale - border.GetPixel (x + 1, y).grayscale;
				bndY [i] = border.GetPixel (x, y).grayscale - border.GetPixel (x, y + 1).grayscale;
				velX [i] = (flow.GetPixel (x, y).grayscale - flow.GetPixel (x + 1, y).grayscale) * 0.1f;
				velY [i] = (flow.GetPixel (x, y).grayscale - flow.GetPixel (x, y + 1).grayscale) * 0.1f;
			}

		}
		color_buffer = new Color[width * height];
	}

    void Update()
	{
		if (color_buffer == null) 
			color_buffer = new Color[width * height];
		// reset values
		for (int i = 0; i < size; i++) {
			dens_prev[i] = 0;
			u_prev[i] = velX[i];
			v_prev[i] = velY[i];
		}
		UserInput();
		vel_step(u, v, u_prev, v_prev, dt);
		dens_step(dens, dens_prev, u, v, dt);
		Draw();
		double rmsd = ComputeRMSD (dens, dens_prev);
//		Debug.Log (string.Format("RMSD = {0}", rmsd));

		float c0 = 0.0001f, c1 = 0.0001f, d0 = 0.009f;
		if (fire_life-- > 0) {
			c0 = 0.004f;
			c1 = 0.003f;
			d0 = 0.045f;
		}
			
		for (int x = (int)(width * 0.4f); x < (int)(width * 0.6f); x++) {
			for (int y = (int)(height * 0.65f); y < (int)(height * 0.75f); y++) {
				int idx = y * (width + 2) + x;
				v [idx] -= (float)(Math.Max (0.0, c1 * (float)Math.Sin (num_frame / 2.0f) + c0));
				dens [idx] += d0;
			}
		}

		if (fade_life-- > 0) {
			for (int x = 0; x <= width+2; x++) {
				for (int y = 0; y <= height + 1; y++) {
					int idx = y * (width + 2) + x;
					dens [idx] *= (fire_life > 15) ? 0.94f : 0.80f;
				}
			}
		}

		num_frame++;
	}
	
	void addFields(float[] x, float[] s, float dt)
	{
		for (int i=0; i<size ; i++ ) {
			x[i] += dt*s[i];
		}
	}

	double ComputeChecksum(float[] a) {
		double blah = 0.0;
		for (int i=0; i<(width+2)*(height+2);i++) blah+=a[i];
		return blah;
	}

	double ComputeRMSD(float[] a, float[] b) {
		double sumsq = 0.0;
		for (int i = 0; i < width + 2; i++) {
			for (int j = 0; j < height + 2; j++) {
				float dif = a [j * (width + 2) + i] - b [j * (width + 2) + i];
				sumsq += dif * dif;
			}
		}
		return Math.Sqrt (sumsq / (width + 2) / (height + 2));
	}

	void set_bnd(int b, float[] x)
	{
		// b/w texture as obstacles
		for (int j = 1; j <= height; j++) {
			for (int i = 1; i <= width; i++) {
				int p = i + j * width;
				if (bndX[p] < 0) {
					x[p] = (b == 1) ? -x[p + 1] : x[p + 1];
				}
				if (bndX[p] > 0) {
					x[p] = (b == 1) ? -x[p - 1] : x[p - 1];
				}
				if (bndY[p] < 0) {
					x[p] = (b == 2) ? -x[p + rowSize] : x[p + rowSize];
				}
				if (bndY[p] > 0) {
					x[p] = (b == 2) ? -x[p - rowSize] : x[p - rowSize];
				}
			}
		}
		// only rect borders as obstacles (but faster)
		/*float sign;
		// left/right: reflect if b is 1, else keep value before edge
		sign = (b == 1) ? -1 : 1;
		for (int j = 1; j <= height; j++) {
			x[j * rowSize] = sign * x[1 + j * rowSize];
			x[(width + 1) + j * rowSize] = sign * x[width + j * rowSize];
		}
		// bottom/top: reflect if b is 2, else keep value before edge
		sign = (b == 2) ? -1 : 1;
		for (int i = 1; i <= width; i++) {
			x[i] = sign * x[i + rowSize];
			x[i + (height + 1) * rowSize] = sign * x[i + height * rowSize];
		}
		// vertices
		int maxEdge = (height + 1) * rowSize;
		x[0]                 = 0.5f * (x[1] + x[rowSize]);
		x[maxEdge]           = 0.5f * (x[1 + maxEdge] + x[height * rowSize]);
		x[(width+1)]         = 0.5f * (x[width] + x[(width + 1) + rowSize]);
		x[(width+1)+maxEdge] = 0.5f * (x[width + maxEdge] + x[(width + 1) + height * rowSize]);*/
	}

	// GAUSS_SEIDEL
	void lin_solve(float[] x, float[] x0, float a, float c)
	{
		if (a == 0 && c == 1) {
			for (int i = 0; i < size; i++) {
				x[i] = x0[i];
			}
			set_bnd(0, x);
		} else {
			for (int k=0 ; k<iterations; k++) {
				for (int j=1 ; j<=height; j++) {
					int lastRow = (j - 1) * rowSize;
					int currentRow = j * rowSize;
					int nextRow = (j + 1) * rowSize;
					float lastX = x[currentRow];
					++currentRow;
					for (int i=1; i<=width; i++)
						lastX = x[currentRow] = (x0[currentRow] + a * (lastX + x[++currentRow] + x[++lastRow] + x[++nextRow])) / c;
				}
				set_bnd(0, x);
			}
		}
	}


	// JACOBIAN
	void lin_solve0(float[] x, float[] x0, float a, float c)
	{

		if (a == 0 && c == 1) {
			for (int i = 0; i < size; i++) {
				x[i] = x0[i];
			}
//			set_bnd(0, x);
		} else {
			float[] tmp = new float[(height + 2) * (width + 2)];
			for (int k=0 ; k<iterations; k++) {
				for (int j=1 ; j<=height; j++) {
					for (int i=1; i<=width; i++) {
						int idx0 = (j - 1) * (width + 2) + i, idx1 = (j + 1) * (width + 2) + i,
						idx2 = j * (width + 2) + (i - 1), idx3 = j * (width + 2) + (i + 1),
						idx4 = j * (width + 2) + i;
						tmp [idx4] = (a * (x [idx0] + x [idx1] + x [idx2] + x [idx3]) + x0 [idx4]) / c;
					}
				}
//				set_bnd(0, tmp);
				Array.Copy (tmp, x, (height + 2) * (width + 2));
			}
		}

	}

	void lin_solve1(float[] x, float[] x0, float a, float c) {

		// Must be called at every lin_solve
		shader.SetBuffer (kernel_id, "x_in", buf_x_in);
		shader.SetBuffer (kernel_id, "x0", buf_x0);
		shader.SetBuffer (kernel_id, "x_out", buf_x_out);
		shader.SetBuffer (kernel_id, "args", buf_args);

		float[] args = { a, c, height, width, 0 };
		buf_args.SetData (args);
		buf_x0.SetData (x0);
		buf_x_in.SetData (x);


		float[] tmp = new float[(height + 2) * (width + 2)];
		buf_x_out.SetData (tmp);

		for (int i = 0; i < iterations; i ++) {
			args [4] = i;
			buf_args.SetData (args);
			shader.Dispatch (kernel_id, 128, 1, 1);
		}
		buf_x_out.GetData (x);

	}

	/**/
	void multiplyASolver(float[] x_in, float[] x_out, float a, float c, int boundary) {
		for (int i=0; i<=width+1; i++) {
			for (int j=0; j<=height+1; j++) {
				if (i == 0 || j == 0 || i == width + 1 || j == height + 1)
					x_out [i * rowSize + j] = x_in [i * rowSize + j];
				else {
					float elt = (c - 4.0f * a) * x_in [i * rowSize + j];
					if (i == 1) {
						if (boundary == 1) {
							elt = elt + a * x_in [1 * rowSize + j] + a * x_in [1 * rowSize + j];
						}
					} else {
						elt = elt - a * x_in [(i - 1) * rowSize + j] + a * x_in [i * rowSize + j];
					}

					if (i == width) {
						if ( boundary == 1) {
							elt = elt + a*x_in[width*rowSize+j] + a*x_in[width*rowSize+j];
						} else { elt = elt - a*x_in[(i+1)*rowSize + j] + a*x_in[i*rowSize + j]; }
					}
							
					if(j == 1) {
						if(boundary == 2) elt = elt + a*x_in[i*rowSize + 1] + a*x_in[i*rowSize + 1];
					} else { elt = elt - a*x_in[i*rowSize + j-1] + a*x_in[i*rowSize + j]; }

					if(j == height) {
						if(boundary == 2) elt = elt + a*x_in[i*rowSize + height] + a*x_in[i*rowSize + j];
					} else { elt = elt - a*x_in[i * rowSize + j+1] + a*x_in[i*rowSize + j]; }

					x_out[i*rowSize + j] = elt;
				}
			}
		}
	}

	float dot(float[] x, float[] y) {
		float ret = 0.0f;
		for (int i=0; i<(height+2)*(width+2);i++) ret+=x[i]*y[i];
		return ret;
	}

	// Conjugate Gradient Algorithm
	void lin_solve_CG(int b, float[] x, float[] x0, float a, float c) {
		int S = rowSize * (height + 2);
		Array.Copy (x, x0, S);
		float[] r = new float[S];

		// r = Ax
		multiplyASolver (x, r, a, c, b);

		// r = b - Ax
		for (int i = 0; i < S; i++) {
			r [i] = x0 [i] - r [i];
		}
			
		float[] p = new float[S];
		Array.Copy (p, r, S);

		float[] q = new float[S];
		float rho, rho_old;

		rho = dot (r, r);

		for (int iter = 0; iter < 5; iter++) {
			if (rho == 0)
				break;
			multiplyASolver (p, q, a, c, b); // q = A p
			float p_dot_q = dot(p, q);
			float alpha = rho / p_dot_q;

			for (int i = 0; i < S; i++) {
				x [i] = x [i] + alpha * p [i];
				r [i] = r [i] - alpha * q [i];
			}

			rho_old = rho;
			rho = dot (r, r);
			float beta = rho / rho_old;
			for (int i = 0; i < S; i++) {
				p [i] = r [i] + beta * p [i];
			}
			set_bnd (b, x);
		}
	}

	
	void diffuse(float[] x, float[] x0)
	{
		lin_solve(x, x0, volatilize, 1 + 4 * volatilize);
		//lin_solve_CG(0, x, x0, volatilize, 1 + 4 * volatilize);
	}
	
	void lin_solve2(float[] x, float[] x0, float[] y, float[] y0, float a, float c)
	{
		if (a == 0 && c == 1) {
			for (int i = 0; i < size; i++) {
				x[i] = x0[i];
				y[i] = y0[i];
			}
			set_bnd(1, x);
			set_bnd(2, y);
		} else {
			for (int k=0 ; k<iterations; k++) {
				for (int j=1 ; j <= height; j++) {
					int lastRow = (j - 1) * rowSize;
					int currentRow = j * rowSize;
					int nextRow = (j + 1) * rowSize;
					float lastX = x[currentRow];
					float lastY = y[currentRow];
					++currentRow;
					for (int i=1; i<=width; i++) {
						lastX = x[currentRow] = (x0[currentRow] + a * (lastX + x[currentRow] + x[lastRow] + x[nextRow])) / c;
						lastY = y[currentRow] = (y0[currentRow] + a * (lastY + y[++currentRow] + y[++lastRow] + y[++nextRow])) / c;
					}
				}
				set_bnd(1, x);
				set_bnd(2, y);
			}
		}
	}
	
	void diffuse2(float[] x, float[] x0, float[] y, float[] y0)
	{
		lin_solve2(x, x0, y, y0, visc, 1 + 4 * visc);
	}
	
	void advect(int b, float[] d, float[] d0, float[] u, float[] v, float dt)
	{
		float dt0 = dt * width;
		float Wp5 = width + 0.5f;
		float Hp5 = height + 0.5f;
		for (int j = 1; j<= height; j++) {
			int pos = j * rowSize;
			for (int i = 1; i <= width; i++) {
				float x = i - dt0 * u[++pos]; 
				float y = j - dt0 * v[pos];
				if (x < 0.5f)
					x = 0.5f;
				else if (x > Wp5)
					x = Wp5;
				int i0 = (int)x;
				int i1 = i0 + 1;
				if (y < 0.5f)
					y = 0.5f;
				else if (y > Hp5)
					y = Hp5;
				int j0 = (int)y;
				int j1 = j0 + 1;
				float s1 = x - i0;
				float s0 = 1 - s1;
				float t1 = y - j0;
				float t0 = 1 - t1;
				int row1 = j0 * rowSize;
				int row2 = j1 * rowSize;
				d[pos] = s0 * (t0 * d0[i0 + row1] + t1 * d0[i0 + row2]) + s1 * (t0 * d0[i1 + row1] + t1 * d0[i1 + row2]);
			}
		}
		set_bnd(b, d);
	}
	
	void project(float[] u, float[] v, float[] p, float[] div)
	{
		float h = -0.5f / Mathf.Sqrt(width * height);
		for (int j = 1; j <= height; j++ ) {
			int row = j * rowSize;
			int previousRow = (j - 1) * rowSize;
			int prevValue = row - 1;
			int currentRow = row;
			int nextValue = row + 1;
			int nextRow = (j + 1) * rowSize;
			for (int i = 1; i <= width; i++ ) {
				div[++currentRow] = h * (u[++nextValue] - u[++prevValue] + v[++nextRow] - v[++previousRow]);
				p[currentRow] = 0;
			}
		}
		set_bnd(0, div);
		set_bnd(0, p);
		
		lin_solve(p, div, 1, 4);

		float scale = 0.5f * width;
		for (int j = 1; j<= height; j++ ) {
			int prevPos = j * rowSize - 1;
			int currentPos = j * rowSize;
			int nextPos = j * rowSize + 1;
			int prevRow = (j - 1) * rowSize;
			int nextRow = (j + 1) * rowSize;
			for (int i = 1; i<= width; i++) {
				u[++currentPos] -= scale * (p[++nextPos] - p[++prevPos]);
				v[currentPos]   -= scale * (p[++nextRow] - p[++prevRow]);
			}
		}
		set_bnd(1, u);
		set_bnd(2, v);
	}
	
	void dens_step(float[] x, float[] x0, float[] u, float[] v, float dt)
	{
		addFields(x, x0, dt);
		diffuse(x0, x);
		advect(0, x, x0, u, v, dt );
	}
	
	void vel_step(float[] u, float[] v, float[] u0, float[] v0, float dt)
	{
		float[] temp;
		addFields(u, u0, dt);
		addFields(v, v0, dt);
		temp = u0; u0 = u; u = temp;
		temp = v0; v0 = v; v = temp;
		diffuse2(u, u0, v, v0);
		project(u, v, u0, v0);
		temp = u0; u0 = u; u = temp; 
		temp = v0; v0 = v; v = temp;
		advect(1, u, u0, u0, v0, dt);
		advect(2, v, v0, u0, v0, dt);
		project(u, v, u0, v0);
	}
	
    public void AddSomething(Vector3 l, Vector3 v) // Should be translated to local by this time
	{
        int x = (int)(l.x * width), y = (int)(l.y * height);
		int i = (x + 1) + (y + 1) * rowSize;
        dens[i] += 3f;
        u[i] += v.x;
        this.v[i] += v.y;
    }

	void UserInput()
	{
		// draw on the water
		bool mBtnLeft = Input.GetMouseButton(0);
		bool mBtnRight = Input.GetMouseButton(1);
		if (mBtnLeft || mBtnRight) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100)) {
				// determine indices where the user clicked
				int x = (int)(hit.point.x * width);
				int y = (int)((hit.point.z + 0.375f) * height);
				int i = (x + 1) + (y + 1) * rowSize;
				if (x < 1 || x > width-1 || y < 1 || y > height-1) return;
				// add or dec density
				dens_prev[i] += mBtnLeft ? 3f : -3f;
				// add velocity
				u_prev[i] += Input.GetAxis("Mouse X") * 0.5f;
				v_prev[i] += Input.GetAxis("Mouse Y") * 0.5f;
			}
		}
	}
	
	void Draw()
	{
		// visualize water
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int i = (x + 1) + (y + 1) * rowSize;
				float d = 5f * dens[i];
				int idx = y * width + x;
				color_buffer [idx].r = u [i] * 20 + bndX [i] + 0.5f;
				color_buffer [idx].g = v [i] * 20 + bndY [i] + 0.5f + d * 0.5f;
				color_buffer [idx].b = 1 + d;
				color_buffer [idx].a = d;
				/*float d = 5f * dens[(x + 1) + (y + 1) * rowSize];
				tex.SetPixel(x, y, new Color(0, d * 0.5f, d));*/
			}
		}
		tex.SetPixels (color_buffer);
		tex.Apply(false);
	}
}
