namespace Main.Gameplay.Entities;

public interface ICombatEntity
{
	Vector2 Position { get; }
	bool IsDead { get; }
	float HurtboxRadius { get; }
	bool ApplyDamage(int amt, CharacterEntity from);    //more on stats, events and real gameplay-based stuff here
	void OnHit(float amt, bool isDead, CharacterEntity from);   //more on juice and polish here
	void OnDeath();
}