///
/// Water Render
/// To gen the index of quadMesh.
/// Date: 2022/04/14 
///
#include <vector>
#include <iostream>
#include <windows.h>

int main()
{
    const size_t X_SEGMENT = 4;
    const size_t Y_SEGMENT = 4;

    int pCount = 2 * (X_SEGMENT - 1) * (Y_SEGMENT - 1); // count of primitive
    int loopCount = X_SEGMENT*Y_SEGMENT;  // traverse pivot (count of pivot)

    std::vector<WORD> indices;

    for(size_t i = 0; i < loopCount; i++)
    {
        // is pivot?
        int boundVal = 4*(i / X_SEGMENT + 1) - 1;
        if (i != boundVal && i / X_SEGMENT != Y_SEGMENT - 1)
        {
            indices.push_back(i);
            indices.push_back(i+X_SEGMENT+1);
            indices.push_back(i + 1);
            indices.push_back(i);
            indices.push_back(i + X_SEGMENT);
            indices.push_back(i + X_SEGMENT + 1);
        }
    }

    // print out 
    for (size_t i  = 0; i < indices.size(); i++)
    {
        std::cout<<indices[i]<<std::endl;
    }

    // for(size_t i = 0; i < indices.size()/3; i++)
    // {
    //     std::cout<< indices[i] << "," << indices[i+1] 
    //     << "," << indices[i+2] << "\n" << std::endl;

    //     std::cout<< indices[i+3] << "," << indices[i+4] 
    //     << "," << indices[i+5] << "\n" << std::endl;
    // }
    return 0;
}