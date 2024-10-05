using Godot;
using System;

public partial class GeneroDatumo : Node
{
    [Export] private int Horizontalo = 0;
    [Export] private int Vertikalo = 0;
    [Export] private MalamikoTipoj Tipo = MalamikoTipoj.Kuriero;

    [Export(PropertyHint.Range, "0,10,.1")]
    private float RelativaGenerTempo = 0f;
}
