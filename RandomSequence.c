#include <stdio.h>
#include <time.h>
#include <Windows.h>

void getRandomSequence_Loop(int *arr, int count)
{
	int n, i, j;
	int dup;
	for (i = 0; i < count; ++i)
	{
		do
		{
			n = rand() % 100 + 1;
			dup = 0;
			
			for (j = 0; j < i && !dup; ++j)
				if (arr[j] == n)
					dup = 1;
		} while (dup);
		
		arr[i] = n;
	}
}

void getRandomSequence_Shuffle(int *arr, int count)
{
	static int PREDEFINED[100];
	static int initialized = 0;
	if (!initialized)
	{
		int i;
		for (i = 0; i < 100; ++i)
			PREDEFINED[i] = i + 1;
		initialized = 1;
	}
	
	int i, j, tmp;
	
	/* shuffle */
	for (i = 0; i < count; ++i)
	{
		j = rand() % 100;
		tmp = PREDEFINED[i];
		PREDEFINED[i] = PREDEFINED[j];
		PREDEFINED[j] = tmp;
	}
	
	/* get values */
	for (i = 0; i < count; ++i)
		arr[i] = PREDEFINED[j];
}


int main(void)
{
	DWORD t;
	float fwhile, fshuffle, fs;

	int arr[10];

	const int TEST_COUNT = 5;

	srand(time(nullptr));

	printf("Test LOOP...");
	getchar();

	fwhile = 0.f;
	fs = 0.f;
	for (int k = 0; k < TEST_COUNT; ++k)
	{
		t = GetTickCount();
		for (int i = 0; i < 1000000; ++i)
			getRandomSequence_Loop(arr, 10);
		t = GetTickCount() - t;

		printf("Test %d: %d ms\n", k + 1, t);
		
		fs += (float)t;
	}
	fwhile = fs / TEST_COUNT;
	printf("\tAverage: %f\n", fwhile);

	printf("Test SHUFFLE...");
	getchar();
	
	fshuffle = 0.f;
	fs = 0.f;
	for (int k = 0; k < TEST_COUNT; ++k)
	{
		t = GetTickCount();
		for (int i = 0; i < 1000000; ++i)
			getRandomSequence_Shuffle(arr, 10);
		t = GetTickCount() - t;

		printf("Test %d: %d ms\n", k + 1, t);
		
		fs += (float)t;
	}
	fshuffle = fs / TEST_COUNT;
	printf("\tAverage: %f\n", fshuffle);

	printf("\nWhile/Shuffle = %f\n", fwhile / fshuffle);
	printf("\nShuffle/While = %f\n", fshuffle / fwhile);

	getchar();
	return 0;
}
