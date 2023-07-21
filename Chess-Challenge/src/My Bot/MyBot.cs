using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900};
    string[,,] openingBook = {{{"e2e4","g1f3"},{"e2e4","b1c3"},{"e2e4","d2d4"}},{{"c7c5","d7d6"},{"e7e6","d7d5"},{"d7d6","g8f6"}}};
    int[] opening = {0, 0};
    Move bestNextMove;
    int bestNextValue = 0;
    
    Random rng = new();
    
    private void UpdateBestNext(Move move, int value) {
        if (value > bestNextValue) {
            bestNextValue = value;
            bestNextMove = move;
        }
    }

    public Move Think(Board board, Timer timer)
    {
        // if (board.GetAllPieceLists().Length < 6) return Endgame(board);
        if (board.PlyCount <= 3) return Opening(board);

        Move[] possibleMoves = board.GetLegalMoves();
        bestNextMove = possibleMoves[rng.Next(possibleMoves.Length)];
        bestNextValue = 0;

        foreach (Move move in possibleMoves) {
            // Rule 1
            // Checkmate if possible
            if (MoveIsCheck(board, move, true)) return move;

            // only consider save moves
            if (IsSafeMove(board, move) || IsDefended(board, move))
            {
                // Rule 2
                // Check if possible
                if (MoveIsCheck(board, move, false)) UpdateBestNext(move, 1000);

                // Capture if possible
                PieceType capture = move.CapturePieceType;
                if (capture > 0) UpdateBestNext(move, pieceValues[(int)capture]);

                
            }
        }

        return bestNextMove;
    }

    private Move Opening(Board board)
    {
        int ply = board.PlyCount;
        if (ply / 2 == 0) opening[ply] = rng.Next(2);
        return new Move(openingBook[ply % 2, opening[ply % 2], ply / 2],board);
    }

    private bool MoveIsCheck(Board board, Move move, bool mate)
    {
        board.MakeMove(move);
        bool condition = mate ? board.IsInCheckmate() : board.IsInCheck();
        board.UndoMove(move);
        return condition;
    }

    /// <summary>
    /// check if target square is not attackt by anything
    /// </summary>
    private bool IsSafeMove(Board board, Move move)
    {
        Square target = move.TargetSquare;
        bool safe = true;
        board.MakeMove(move);
        foreach (Move counterMove in board.GetLegalMoves(true))
        {
            if (counterMove.TargetSquare == target)
            {
                safe = false;
                break;
            }
        }
        board.UndoMove(move);
        return safe;
    }

    /// <summary>
    /// check if target square is defended
    /// </summary>
    private bool IsDefended(Board board, Move move)
    {
        Square targetSquare = move.TargetSquare;
        board.MakeMove(move);
        bool defended = true;
        foreach (Move opponentMove in board.GetLegalMoves(true))
        {
            if (IsSafeMove(board, opponentMove))
            {
                defended = false;
                break;
            }
        }
        board.UndoMove(move);
        return defended;
    }
}