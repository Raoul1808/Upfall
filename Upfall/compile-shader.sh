shader_in="Shaders/$1.fx"
shader_out="Content/$1.fxb"
wine /home/mew/.wine/drive_c/Program\ Files\ \(x86\)/Microsoft\ DirectX\ SDK\ \(June\ 2010\)/Utilities/bin/x64/fxc.exe '/Vd' '/T' 'fx_2_0' '/Fo' ${shader_out} ${shader_in}
