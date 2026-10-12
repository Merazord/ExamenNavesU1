from pathlib import Path
import wave
import numpy as np

rate = 22050
beat = .5
duration = 32
mix = np.zeros(int(rate * duration), dtype=np.float64)
rng = np.random.default_rng(27)
chords = [(50,53,57), (46,50,53), (53,57,60), (48,52,55)]

def hz(note): return 440 * 2 ** ((note - 69) / 12)
def add(start, seconds, note, volume, voice):
    t = np.arange(int(seconds * rate)) / rate
    f = hz(note)
    env = np.minimum(1, t / .025) * np.minimum(1, (seconds - t) / .08)
    if voice == 'strings':
        signal = sum(np.sin(2 * np.pi * f * h * t) / (h * h) for h in range(1, 6))
        signal += .25 * np.sin(2 * np.pi * f * 1.004 * t)
    elif voice == 'brass':
        signal = sum(np.sin(2 * np.pi * f * h * t) / h for h in range(1, 5))
        env *= np.exp(-t * 1.7)
    else:
        signal = np.sin(2 * np.pi * f * t) + .2 * np.sin(2 * np.pi * f * 2 * t)
    begin = int(start * rate); length = min(len(t),len(mix)-begin)
    mix[begin:begin+length] += signal[:length] * env[:length] * volume

melody = [74,77,81,79,77,74,72,69,70,74,77,81,79,77,74,70,
          77,81,84,81,79,77,76,74,72,76,79,77,76,72,69,72]
for bar in range(16):
    chord = chords[bar % 4]
    for note in chord: add(bar*2,2,note+12,.045,'strings')
    for step in range(8):
        add(bar*2+step*.25,.22,chord[step%3]+12,.065,'strings')
    for step in range(4):
        add(bar*2+step*.5,.43,chord[0]-12,.14,'bass')
        add(bar*2+step*.5,.43,melody[(bar*4+step)%32],.11,'brass')
        t = np.arange(int(.22*rate))/rate
        kick = np.sin(2*np.pi*(75*t-70*t*t))*np.exp(-t*20)*.25
        begin=int((bar*2+step*.5)*rate)
        mix[begin:begin+len(t)] += kick
        if step%2:
            noise=rng.uniform(-1,1,len(t))*np.exp(-t*28)*.10
            mix[begin:begin+len(t)] += noise
    if bar%4 == 0:
        t=np.arange(int(.7*rate))/rate
        crash=rng.uniform(-1,1,len(t))*np.exp(-t*6)*.08
        begin=int(bar*2*rate); mix[begin:begin+len(t)] += crash
# Normalizacion con margen para evitar saturacion.
mix *= .88 / max(np.max(np.abs(mix)), .001)
mix[:220] *= np.linspace(0,1,220)
mix[-220:] *= np.linspace(1,0,220)
target=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio/music.wav'
with wave.open(str(target),'wb') as w:
    w.setparams((1,2,rate,0,'NONE','not compressed'))
    w.writeframes((mix*32767).astype('<i2').tobytes())
print('Musica original:',duration,'segundos; pico:',round(float(np.max(np.abs(mix))),3))
