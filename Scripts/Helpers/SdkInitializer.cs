using System.Collections;
using CorePublic.Interfaces;
using UnityEngine;

namespace CorePublic.Helpers
{
    /// <summary>
    /// One step of the startup chain: initializes an SDK (analytics, ads, consent, ...)
    /// after its <see cref="dependencies"/> are initialized. As an <see cref="IPrerequisite"/>
    /// it holds the game start until <see cref="IsInitialized"/> returns true.
    /// Renamed from AnalyticInitializer in 0.4.0.
    /// </summary>
    public abstract class SdkInitializer: MonoBehaviour, IPrerequisite
    {
        public SdkInitializer[] dependencies;
        public bool initializeAtStart = true;
        public abstract bool IsInitialized();
        public abstract void Initialize();

        private IEnumerator Start()
        {
            if (initializeAtStart)
            {
                foreach (SdkInitializer dependency in dependencies)
                {
                    while (!dependency.IsInitialized())
                    {
                        yield return null;
                    }
                }
                Initialize();
            }
        }

        public bool IsMet()
        {
            return IsInitialized();
        }
    }
}