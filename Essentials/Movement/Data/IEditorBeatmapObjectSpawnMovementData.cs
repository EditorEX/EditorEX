﻿using BeatmapEditor3D.DataModels;
using UnityEngine;

namespace BetterEditor.Essentials.Movement.Data
{
	public interface IEditorBeatmapObjectSpawnMovementData
	{
		void Init(int noteLinesCount, float startNoteJumpMovementSpeed, float startBpm, BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, float noteJumpValue, IJumpOffsetYProvider jumpOffsetYProvider, Vector3 rightVec, Vector3 forwardVec);

		BeatmapObjectSpawnMovementData.ObstacleSpawnData GetObstacleSpawnData(ObstacleEditorData obstacleData);

		BeatmapObjectSpawnMovementData.NoteSpawnData GetJumpingNoteSpawnData(NoteEditorData noteData);

		BeatmapObjectSpawnMovementData.SliderSpawnData GetSliderSpawnData(BaseSliderEditorData sliderData);

		Vector3 GetNoteOffset(int noteLineIndex, NoteLineLayer noteLineLayer);

		Vector3 GetObstacleOffset(int noteLineIndex, NoteLineLayer noteLineLayer);

		float JumpPosYForLineLayerAtDistanceFromPlayerWithoutJumpOffset(NoteLineLayer lineLayer, float distanceFromPlayer);

		float HighestJumpPosYForLineLayer(NoteLineLayer lineLayer);

		float HighestJumpPosYForLineLayerWithoutJumpOffset(NoteLineLayer lineLayer);

		float NoteJumpGravityForLineLayer(NoteLineLayer lineLayer, NoteLineLayer beforeJumpLineLayer);

		float NoteJumpGravityForLineLayerWithoutJumpOffset(NoteLineLayer lineLayer, NoteLineLayer beforeJumpLineLayer);
	}
}