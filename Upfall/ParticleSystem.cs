using System;
using System.Collections.Generic;
using Brocco;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public static class ParticleSystem
{
    private class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float TimeToLive;
    }

    private static float _deathParticleTimer = 0f;

    private static Random _random = new Random();
    
    private static List<Particle> _particles = new();

    public static void UpdateParticles(float dt)
    {
        if (_deathParticleTimer > 0f)
            _deathParticleTimer -= dt;
        
        for (int i = 0; i < _particles.Count;)
        {
            var particle = _particles[i];
            if (particle.TimeToLive <= 0f)
                _particles.RemoveAt(i);
            else
            {
                i++;
                particle.TimeToLive -= dt;
                particle.Position += particle.Velocity * dt;
            }
        }
    }

    public static void RenderParticles(SpriteBatch spriteBatch)
    {
        foreach (var particle in _particles)
        {
            spriteBatch.Draw(Assets.Pixel, particle.Position, null, Color.White, 0f, new Vector2(0.5f), particle.Scale, SpriteEffects.None, 1f);
        }
    }

    private static float GetRandomBetweenFloats(float min, float max)
    {
        return _random.NextSingle() * (max - min) + min;
    }

    public static void SpawnDeathParticles(Vector2 pos)
    {
        const float deathParticleMin = 0.25f;
        const float deathParticleMax = 0.55f;
        const float particleVelocity = 30f;
        const float particleScale = 2f;

        _deathParticleTimer = deathParticleMax;
        
        var amount = _random.Next(10, 14);
        for (int i = 0; i < amount; i++)
        {
            int xOffset = _random.Next(-1, 2);
            int yOffset = _random.Next(-1, 2);
            float angle = _random.NextSingle() * MathHelper.TwoPi;
            var vel = Vector2.Transform(Vector2.UnitX * particleVelocity, Matrix.CreateRotationZ(angle));
            float ttl = GetRandomBetweenFloats(deathParticleMin, deathParticleMax);
            _particles.Add(new()
            {
                Position = new Vector2(pos.X + xOffset, pos.Y + yOffset),
                Velocity = vel,
                Scale = particleScale,
                TimeToLive = ttl,
            });
        }
    }

    public static bool DeathParticlesDone()
    {
        return _deathParticleTimer <= 0f;
    }
}
