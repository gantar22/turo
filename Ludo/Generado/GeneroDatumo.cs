using Godot;
using System;

public partial class GeneroDatumo : Node
{
    [Export] public int Horizontalo = 0;
    [Export] public int Vertikalo = 0;
    [Export] public MalamikoTipoj Tipo = MalamikoTipoj.Kuriero;

    [Export(PropertyHint.Range, "0,8,1")]
    public int RelativaGenerTempo = 0;
}
